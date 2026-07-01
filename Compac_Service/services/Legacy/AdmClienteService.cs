using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.DTOs.ServiceResponse;
using back_cabs.CRM.Interfaces.Auth;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.Middleware;
using back_cabs.CRM.models.Auth;
using back_cabs.CRM.models.legacy;
using CRM.DTOs.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio para clientes con domicilios de Adminpaq
    /// OPTIMIZADO CON REDIS para búsquedas rápidas
    /// </summary>
    public class AdmClienteService : IAdmClienteService
    {
        private readonly IAdmClienteRepository _repository;
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ReadOnlyContext _authContext;
        private readonly WriteContext _writeContext;
        private readonly IUsuarioAuthRepository _repositoryAuth;
        private readonly ILogger<AdmClienteService> _logger;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _config;

        // Tiempos de caché
        private readonly TimeSpan _cacheDurationBusqueda = TimeSpan.FromMinutes(10);

        public AdmClienteService(
            IAdmClienteRepository repository,
            IUsuarioAuthRepository repositoryAuth,
            LegacyCompacReadOnlyContext context,
            ReadOnlyContext authContext,
            WriteContext writeContext,
            ILogger<AdmClienteService> logger,
            ICacheService cacheService,
            IConfiguration config)
        {
            _repository = repository;
            _context = context;
            _authContext = authContext;
            _writeContext = writeContext;
            _repositoryAuth = repositoryAuth;
            _logger = logger;
            _cacheService = cacheService;
            _config = config;
        }

        private string? GetRutaConstancias()
        {
            var ruta = _config["RUTA_CONSTANCIAS"];
            if (!string.IsNullOrEmpty(ruta)) return ruta;

            ruta = Environment.GetEnvironmentVariable("RUTA_CONSTANCIAS");
            if (!string.IsNullOrEmpty(ruta)) return ruta;

            try
            {
                var dir = Directory.GetCurrentDirectory();
                for (int i = 0; i < 4; i++)
                {
                    var envPath = Path.Combine(dir, ".env");
                    if (File.Exists(envPath))
                    {
                        var lines = File.ReadAllLines(envPath);
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                                continue;

                            var parts = line.Split('=', 2);
                            if (parts.Length == 2 && parts[0].Trim() == "RUTA_CONSTANCIAS")
                            {
                                return parts[1].Trim().Trim('"').Trim('\'');
                            }
                        }
                    }
                    var parent = Directory.GetParent(dir);
                    if (parent == null) break;
                    dir = parent.FullName;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al intentar leer el archivo .env para buscar RUTA_CONSTANCIAS");
            }

            return null;
        }

        /// <summary>
        /// Registro de clientes nuevos con domicilio
        /// </summary>
        public async Task<ServiceResult<AdmClienteConDomicilioResponseDto>> RegistrarAsync(UserClientRequestDto clientData)
        {
            string rutaBaseGuardada = "";
            string tituloArchivo = "";
            string nombreArchivoOriginal = "";

            if (clientData.ConstanciaFiscal != null && clientData.ConstanciaFiscal.Length > 0)
            {
                nombreArchivoOriginal = Path.GetFileName(clientData.ConstanciaFiscal.FileName);
                tituloArchivo = $"CSF{clientData.RFC}";

                var rutaBase = GetRutaConstancias();
                if (string.IsNullOrEmpty(rutaBase))
                {
                    _logger.LogWarning("No se definió la variable de entorno RUTA_CONSTANCIAS. Usando ruta por defecto 'wwwroot/constancias'.");
                    rutaBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "constancias");
                }

                rutaBaseGuardada = rutaBase;

                if (!Directory.Exists(rutaBase))
                {
                    Directory.CreateDirectory(rutaBase);
                }

                var nombreArchivoDestino = nombreArchivoOriginal;
                var rutaArchivoGuardado = Path.Combine(rutaBase, nombreArchivoDestino);

                try
                {
                    using (var stream = new FileStream(rutaArchivoGuardado, FileMode.Create))
                    {
                        await clientData.ConstanciaFiscal.CopyToAsync(stream);
                    }
                    _logger.LogInformation("Archivo de constancia fiscal guardado exitosamente en: {Ruta}", rutaArchivoGuardado);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar el archivo de constancia fiscal");
                    return new ServiceResult<AdmClienteConDomicilioResponseDto>
                    {
                        Success = false,
                        Message = "Error al guardar el archivo de constancia fiscal en el servidor."
                    };
                }
            }

            //Validar existencia de RFC o email para evitar duplicados
            var existeRfc = await _context.AdmClientes
                .AsNoTracking()
                .AnyAsync(c => c.CRfc == clientData.RFC);
            if (existeRfc)
            {

                _logger.LogWarning("Intento de registro con RFC duplicado: {RFC}", clientData.RFC);
            }


            var nuevoCliente = new AdmCliente
            {
                //Datos Personales
                CCodigoCliente = clientData.RFC, // Usamos RFC como código cliente para garantizar unicidad
                CCodigoAlterno = string.Empty,
                CRazonSocial = $"{clientData.Nombre} {clientData.ApellidoPaterno} {clientData.ApellidoMaterno}".Trim(),
                CRfc = clientData.RFC,
                CCurp = clientData.CURP,

                //Datos Financieros y de contacto
                CIdMoneda = 0,
                CListaPrecioCliente = 0,
                CDescuentoDocto = 0,
                CDescuentoMovto = 0,
                CIdValorClasifCliente1 = 0,
                CIdValorClasifCliente2 = 0,
                CIdValorClasifCliente3 = 0,
                CIdValorClasifCliente4 = 0,
                CIdValorClasifCliente5 = 0,
                CIdValorClasifCliente6 = 0,
                CTipoCliente = 1, // Asumimos tipo cliente
                CEstatus = 0, // Inactivo hasta que se apruebe

                // Datos iniciales de credito
                CBanVentaCredito = 1, // Asumimos que el nuevo cliente puede tener crédito
                CLimiteCreditoCliente = 0,
                CDiasCreditoCliente = 0,
                CBanExcederCredito = 0,
                CDescuentoProntoPago = 0,
                CDiasProntoPago = 0,
                CInteresMoratorio = 0,
                CDiaPago = 31,
                CDiasRevision = 31,
                CDiasEmbarqueCliente = 0,
                CIdAlmacen = 0,
                CIdAgenteVenta = 0,
                CIdAgenteCobro = 0,
                CRestriccionAgente = 0,
                CImpuesto1 = 0,
                CImpuesto2 = 0,
                CImpuesto3 = 0,
                CRetencionCliente1 = 0,
                CRetencionCliente2 = 0,
                CIdValorClasifProveedor1 = 0,
                CIdValorClasifProveedor2 = 0,
                CIdValorClasifProveedor3 = 0,
                CIdValorClasifProveedor4 = 0,
                CIdValorClasifProveedor5 = 0,
                CLimiteCreditoProveedor = 0,
                CDiasCreditoProveedor = 0,
                CTiempoEntrega = 0,
                CDiasEmbarqueProveedor = 0,
                CImpuestoProveedor1 = 0,
                CImpuestoProveedor2 = 0,
                CImpuestoProveedor3 = 0,
                CRetencionProveedor1 = 0,
                CRetencionProveedor2 = 0,
                CBanInteresMoratorio = 0,
                CComVentaExcepCliente = 0,
                CComCobroExcepCliente = 0,
                CBanProductoConsignacion = 0,
                CSegContCliente1 = "",
                CSegContCliente2 = "",
                CSegContCliente3 = "",
                CSegContCliente4 = "",
                CSegContCliente5 = "",
                CSegContCliente6 = "",
                CSegContCliente7 = "",
                CSegContProveedor1 = "",
                CSegContProveedor2 = "",
                CSegContProveedor3 = "",
                CSegContProveedor4 = "",
                CSegContProveedor5 = "",
                CSegContProveedor6 = "",
                CSegContProveedor7 = "",
                CBanDomicilio = 0,
                CBanCreditoYCobranza = 0,
                CBanEnvio = 0,
                CBanAgente = 0,
                CBanImpuesto = 0,
                CBanPrecio = 0,
                CFacTerc01 = 0,
                CComVenta = 0,
                CComCobro = 0,
                CIdMoneda2 = 0,
                CTipoEntre = 0,
                CConcTeEma = 0,
                CFtoAddend = 0,
                CIdCertCte = 0,
                CEncripEnt = 0,
                CBanCfd = 0,
                CIdAddenda = -1,
                CCodProvCo = "",
                CEnvAcUse = 0,
                CCon1Nom = "",
                CCon1Tel = "",
                CQuitaBlan = 0,
                CFmtoEntre = 0,
                CIdComplem = -1,
                CDesglosaI2 = 0,
                CLimDoctos = 0,
                CSitioFtp = "",
                CUsrFtp = "",
                CMetodoPag = "",
                CNumCtaPag = "",
                CIdCuenta = 0,
                CUsoCfdi = "",
                CRegimFisc = "",

                //Datos de Contacto
                CEmail1 = clientData.Email,
                CEmail2 = clientData.Email2 ?? string.Empty,
                CEmail3 = clientData.Email3 ?? string.Empty,
                CWhatsapp = clientData.Telefono,

                //Campos Extra
                CTextoExtra1 = clientData.Contraseña, // Contraseña cifrada con SHA-256
                CTextoExtra2 = "Cliente registrado desde sitio web",
                CTextoExtra3 = rutaBaseGuardada.Length > 50 ? rutaBaseGuardada.Substring(0, 50) : rutaBaseGuardada,
                CTextoExtra4 = tituloArchivo.Length > 50 ? tituloArchivo.Substring(0, 50) : tituloArchivo,
                CTextoExtra5 = nombreArchivoOriginal.Length > 50 ? nombreArchivoOriginal.Substring(0, 50) : nombreArchivoOriginal,
                CImporteExtra1 = 0,
                CImporteExtra2 = 0,
                CImporteExtra3 = 0,
                CImporteExtra4 = 0,
                CImporteExtra5 = 0,
                CFechaExtra = DateTime.Now,

                //Fechas
                CFechaAlta = DateTime.Now,
                CFechaBaja = new DateTime(1899, 12, 30),
                CFechaUltimaRevision = DateTime.Now,
            };
            var clienteInsertado = await _repository.InsertAsync(nuevoCliente);

            //Insertar contraseña hasheada en tabla auth_usuarios-clientes
            var password = new Auth_cliente
            {
                Id_Cliente = clienteInsertado.CIdClienteProveedor,
                password = ApiUtilities.GenerateSha256Hash(clientData.Contraseña)
            };
            var passInsertado = await _repositoryAuth.InsertPassword(password);

            _logger.LogInformation("Nuevo cliente registrado con ID {IdCliente} y RFC {RFC}", clienteInsertado.CIdClienteProveedor, nuevoCliente.CRfc);

            // Crear domicilio predeterminado para el cliente
            var nuevoDomicilio = new AdmDomicilio
            {
                CIdCatalogo = clienteInsertado.CIdClienteProveedor,
                CTipoCatalogo = 1, // Cliente
                CTipoDireccion = 0, // 0 Fiscal / 1 Normal
                CNombreCalle = clientData.Direccion?.Calle ?? "",
                CNumeroExterior = clientData.Direccion?.NumeroExterior ?? "",
                CNumeroInterior = clientData.Direccion?.NumeroInterior ?? "",
                CColonia = clientData.Direccion?.Colonia ?? "",
                CCodigoPostal = clientData.Direccion?.CodigoPostal ?? "",
                CCiudad = clientData.Direccion?.Ciudad ?? "",
                CMunicipio = clientData.Direccion?.Municipio ?? "",
                CEstado = clientData.Direccion?.Estado ?? "",
                CPais = clientData.Direccion?.Pais ?? "",

                //Datos de contacto del domicilio
                CTelefono1 = clientData.Telefono,
                CTelefono2 = clientData.Direccion?.Telefono2 ?? "",
                CTelefono3 = clientData.Direccion?.TelefonoCompleto ?? "",
                CTelefono4 = clientData.Telefono, // También guardamos el teléfono principal del cliente


            };
            var domicilioInsertado = await _repository.InsertDomicilioAsync(nuevoDomicilio);

            _logger.LogInformation("Nuevo domicilio registrado con ID {IdDomicilio}", domicilioInsertado.CIdDireccion);

            return new ServiceResult<AdmClienteConDomicilioResponseDto>
            {
                Success = true,
                Message = "Cliente registrado exitosamente",
                Data = MapToDto(clienteInsertado, domicilioInsertado, true)
            };
        }

        public async Task<ServiceResult<AdmClienteConDomicilioResponseDto>> AutorizarClienteAsync(int idCliente, AutorizarClienteRequestDto autorizacionData)
        {
            // Buscar el cliente por ID
            var cliente = await _context.AdmClientes
                .FirstOrDefaultAsync(c => c.CIdClienteProveedor == idCliente);

            if (cliente == null)
            {
                _logger.LogWarning("Intento de autorización de cliente inexistente con ID: {IdCliente}", idCliente);
                return new ServiceResult<AdmClienteConDomicilioResponseDto>
                {
                    Success = false,
                    Message = $"No se encontró el cliente con ID {idCliente}"
                };
            }

            if (cliente.CEstatus == 1)
            {
                _logger.LogWarning("Intento de autorización de cliente ya activo con ID: {IdCliente}", idCliente);
                return new ServiceResult<AdmClienteConDomicilioResponseDto>
                {
                    Success = false,
                    Message = "El cliente ya se encuentra autorizado/activo"
                };
            }

            // ── Datos financieros ──────────────────────────────────────────
            cliente.CListaPrecioCliente = autorizacionData.ListaPrecio ?? cliente.CListaPrecioCliente;
            cliente.CLimiteCreditoCliente = autorizacionData.LimiteCredito ?? cliente.CLimiteCreditoCliente;
            cliente.CDiasCreditoCliente = autorizacionData.DiasCredito ?? cliente.CDiasCreditoCliente;
            cliente.CBanVentaCredito = autorizacionData.PermiteCredito ?? cliente.CBanVentaCredito;
            cliente.CBanExcederCredito = autorizacionData.PuedeExcederCredito ?? cliente.CBanExcederCredito;
            cliente.CDescuentoDocto = autorizacionData.DescuentoDocto ?? cliente.CDescuentoDocto;
            cliente.CDescuentoMovto = autorizacionData.DescuentoMovto ?? cliente.CDescuentoMovto;

            // ── Estatus y entrega ──────────────────────────────────────────
            cliente.CEstatus = 1; // Activo — siempre se activa al autorizar
            cliente.CTipoEntre = autorizacionData.TipoEntrega ?? cliente.CTipoEntre;

            // ── Fecha de revisión ──────────────────────────────────────────
            cliente.CFechaUltimaRevision = DateTime.Now;

            await _repository.UpdateAsync(cliente);

            _logger.LogInformation(
                "Cliente con ID {IdCliente} autorizado exitosamente. Límite crédito: {Limite}, Días crédito: {Dias}",
                idCliente,
                cliente.CLimiteCreditoCliente,
                cliente.CDiasCreditoCliente);

            // Recuperar domicilio para armar el DTO de respuesta
            var domicilio = await _context.AdmDomicilios
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.CIdCatalogo == idCliente && d.CTipoCatalogo == 1);

            return new ServiceResult<AdmClienteConDomicilioResponseDto>
            {
                Success = true,
                Message = "Cliente autorizado exitosamente",
                Data = MapToDto(cliente, domicilio, true)
            };
        }

        /// <summary>
        /// Búsqueda paginada de clientes con domicilio
        /// OPTIMIZADO: Usa Redis para cachear resultados de búsquedas frecuentes
        /// </summary>
        public async Task<(List<AdmClienteConDomicilioResponseDto> Clientes, int TotalRegistros, int TotalPaginas)> SearchPaginatedAsync(AdmClienteFilterDto filter)
        {
            try
            {
                // Generar clave de caché basada en los filtros
                var cacheKey = $"AdmClientes:Search:{filter.NumeroPagina}:{filter.TamanoPagina}:{filter.CodigoCliente}:{filter.RazonSocial}:{filter.RFC}:{filter.Estatus}:{filter.TipoDireccion}";

                // Intentar obtener desde caché
                var cachedResult = await _cacheService.GetAsync<(List<AdmClienteConDomicilioResponseDto>, int, int)>(cacheKey);
                if (cachedResult != default)
                {
                    _logger.LogInformation("✅ Cache HIT - Clientes obtenidos desde Redis. Key: {CacheKey}, Count: {Count}",
                        cacheKey, cachedResult.Item1.Count);
                    return cachedResult;
                }

                _logger.LogInformation("🔍 Cache MISS - Buscando clientes desde BD. Página: {Pagina}, Tamaño: {Tamanio}, CodigoCliente: {Codigo}, RazonSocial: {Razon}",
                    filter.NumeroPagina, filter.TamanoPagina, filter.CodigoCliente ?? "null", filter.RazonSocial ?? "null");

                var (clientes, total) = await _repository.SearchPaginatedAsync(filter);

                // Cargar domicilios para los clientes encontrados
                var clientesConDomicilio = await CargarDomiciliosAsync(clientes, filter.TipoDireccion, filter.IncluirDetalleUbicacion);

                var totalPaginas = (int)Math.Ceiling(total / (double)filter.TamanoPagina);

                var resultado = (clientesConDomicilio, total, totalPaginas);

                // Guardar en caché
                await _cacheService.SetAsync(cacheKey, resultado, _cacheDurationBusqueda);
                _logger.LogInformation("💾 Resultado guardado en Redis. Count: {Count}, Key: {CacheKey}", clientesConDomicilio.Count, cacheKey);

                _logger.LogInformation("✅ Búsqueda de clientes completada. Total: {Total}, Retornados: {Retornados}",
                    total, clientesConDomicilio.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar clientes");
                throw;
            }
        }

        /// <summary>
        /// Obtener cliente por ID con domicilio
        /// </summary>
        public async Task<AdmClienteConDomicilioResponseDto?> GetByIdAsync(int idCliente, bool incluirDetalleUbicacion = true)
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo cliente {IdCliente}", idCliente);

                var cliente = await _repository.GetByIdWithDomicilioAsync(idCliente);
                if (cliente == null)
                {
                    _logger.LogWarning("⚠️ Cliente {IdCliente} no encontrado", idCliente);
                    return null;
                }

                var domicilios = await CargarDomiciliosAsync(new List<AdmCliente> { cliente }, null, incluirDetalleUbicacion);

                _logger.LogInformation("✅ Cliente {IdCliente} obtenido exitosamente", idCliente);

                return domicilios.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cliente {IdCliente}", idCliente);
                throw;
            }
        }

        /// <summary>
        /// Obtener credito y estado de credito del cliente
        /// </summary>
        public async Task<CreditClientDto?> GetCreditByIdAsync(int idCliente)
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo cliente {IdCliente}", idCliente);

                var cliente = await _repository.GetByIdWithDomicilioAsync(idCliente);
                if (cliente == null)
                {
                    _logger.LogWarning("⚠️ Cliente {IdCliente} no encontrado", idCliente);
                    return null;
                }

                return new CreditClientDto
                {
                    LimiteCredito = cliente.CLimiteCreditoCliente,
                    LimiteDocs = cliente.CLimDoctos,
                    DiasCredito = cliente.CDiasCreditoCliente,
                    ExcederCredito = cliente.CBanExcederCredito
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cliente {IdCliente}", idCliente);
                throw;
            }
        }

        public async Task<AdmCliente?> ValidateCredentialsAsync(string email, string contrasena)
        {
            // Validar en el repositorio de clientes para soporte legacy
            var cliente = await _repository.GetByEmailAsync(email);
            if (cliente == null)
            {
                _logger.LogWarning("⚠️ Credenciales inválidas para email: {Email}", email);
                return null;
            }
            // Validar que el cliente esté activo (CEstatus = 1 activo, CEstatus = 0 inactivo/pendiente)
            if (cliente.CEstatus != 1)
            {
                _logger.LogWarning("⚠️ Cliente {IdCliente} inactivo o pendiente de autorización", cliente.CIdClienteProveedor);
                throw new UnauthorizedAccessException("Tu cuenta está inactiva. Contacta a tu administrador.");
            }

            // ValidateClientCredentialsAsync retorna ServiceResult<Auth_cliente>, nunca null.
            // Se debe verificar ServiceResult.Success para saber si las credenciales son correctas.
            var resultado = await _repositoryAuth.ValidateClientCredentialsAsync(cliente.CIdClienteProveedor, contrasena);
            if (!resultado.Success)
            {
                _logger.LogWarning("⚠️ Credenciales inválidas para cliente ID: {IdCliente}, Email: {Email} — Razón: {Mensaje}",
                    cliente.CIdClienteProveedor, email, resultado.Message);
                return null;
            }

            _logger.LogInformation("✅ Credenciales válidas para cliente ID: {IdCliente}, Email: {Email}", cliente.CIdClienteProveedor, email);
            return cliente;
        }

        /// <summary>
        /// Actualiza la contraseña cifrada de un cliente legacy (CTextoExtra1 con SHA-256)
        /// </summary>
        public async Task<bool> ActualizarContrasenaAsync(string email, string nuevaPassword)
        {
            try
            {
                var cliente = await _context.AdmClientes
                    .FirstOrDefaultAsync(c =>
                        c.CEmail1.ToLower() == email.ToLower());

                if (cliente == null)
                {
                    _logger.LogWarning("⚠️ Cliente no encontrado para actualizar contraseña: {Email}", email);
                    return false;
                }
                _logger.LogWarning("⚠️ CONTRASEÑA: {nuevaPassword}", nuevaPassword);
                cliente.CTextoExtra1 = nuevaPassword;
                await _repository.UpdateAsync(cliente);
                _logger.LogInformation("Cliente actualizado: {client}", cliente);

                _logger.LogInformation("Contraseña Actualizada para los datos del cliente, actualizando contraseña hash");

                var AuthCliente = await _authContext.Auth_Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id_Cliente == cliente.CIdClienteProveedor);

                if (AuthCliente != null)
                {
                    var password = ApiUtilities.GenerateSha256Hash(nuevaPassword);
                    await _repositoryAuth.UpdateCredentialsAsync(AuthCliente.Id_Cliente, password);
                    _logger.LogInformation("Contraseña Actualizada en Auth_Clientes para cliente: {Email}", email);
                }

                _logger.LogInformation("✅ Contraseña actualizada para cliente legacy: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar contraseña de cliente legacy: {Email}", email);
                throw;
            }
        }


        /// <summary>
        /// Cargar domicilios para una lista de clientes (evitando OPENJSON con foreach)
        /// </summary>
        private async Task<List<AdmClienteConDomicilioResponseDto>> CargarDomiciliosAsync(
            List<AdmCliente> clientes,
            int? tipoDireccion,
            bool incluirDetalle)
        {
            if (!clientes.Any()) return new List<AdmClienteConDomicilioResponseDto>();

            // Cargar domicilios iterando por cada cliente (evitar Contains/OPENJSON)
            var todosDomicilios = new List<AdmDomicilio>();

            foreach (var cliente in clientes)
            {
                var domiciliosQuery = _context.AdmDomicilios
                    .AsNoTracking()
                    .Where(d => d.CIdCatalogo == cliente.CIdClienteProveedor);

                if (tipoDireccion.HasValue)
                {
                    domiciliosQuery = domiciliosQuery.Where(d => d.CTipoDireccion == tipoDireccion.Value);
                }

                var doms = await domiciliosQuery
                    .OrderBy(d =>
                        // Prioridad: 0=Predeterminado, luego 1=Fiscal, luego otros
                        d.CTipoDireccion == 0 ? 0 :
                        d.CTipoDireccion == 1 ? 1 : 99
                    )
                    .ThenBy(d => d.CIdDireccion)
                    .ToListAsync();

                if (doms.Any())
                {
                    _logger.LogInformation($"🏠 Cliente {cliente.CIdClienteProveedor} (Tipo: {cliente.CTipoCliente}) tiene {doms.Count} domicilio(s). Tipos: {string.Join(", ", doms.Select(d => d.CTipoDireccion))}");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Cliente {cliente.CIdClienteProveedor} (Tipo: {cliente.CTipoCliente}) NO tiene domicilios");
                }

                todosDomicilios.AddRange(doms);
            }

            // Agrupar domicilios por cliente
            var domiciliosPorCliente = todosDomicilios
                .GroupBy(d => d.CIdCatalogo)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Mapear a DTOs - GENERAR UN REGISTRO POR CADA DOMICILIO
            var resultados = new List<AdmClienteConDomicilioResponseDto>();

            foreach (var cliente in clientes)
            {
                var domiciliosList = domiciliosPorCliente.ContainsKey(cliente.CIdClienteProveedor)
                    ? domiciliosPorCliente[cliente.CIdClienteProveedor]
                    : new List<AdmDomicilio>();

                if (domiciliosList.Any())
                {
                    // Crear un registro por cada domicilio
                    foreach (var domicilio in domiciliosList)
                    {
                        resultados.Add(MapToDto(cliente, domicilio, incluirDetalle));
                    }
                }
                else
                {
                    // Si no tiene domicilios, crear un registro sin domicilio
                    resultados.Add(MapToDto(cliente, null, incluirDetalle));
                }
            }

            return resultados;
        }

        /// <summary>
        /// Mapear cliente y domicilio a DTO
        /// </summary>
        private AdmClienteConDomicilioResponseDto MapToDto(AdmCliente cliente, AdmDomicilio? domicilio, bool incluirDetalle)
        {
            var dto = new AdmClienteConDomicilioResponseDto
            {
                Id = cliente.CIdClienteProveedor,
                CodigoCliente = cliente.CCodigoCliente,
                Nombre = cliente.CRazonSocial,
                RFC = !string.IsNullOrWhiteSpace(cliente.CRfc) ? cliente.CRfc : "Sin RFC",
                Telefono = ObtenerTelefono(cliente, domicilio),
                Email = cliente.CEmail1,
                Email2 = cliente.CEmail2,
                Email3 = cliente.CEmail3,
                Estado = domicilio?.CEstado ?? "Sin estado",
                Ubicacion = ConstruirUbicacion(domicilio)
            };

            if (incluirDetalle && domicilio != null)
            {
                dto.UbicacionDetalle = new UbicacionDetalleDto
                {
                    Calle = domicilio.CNombreCalle,
                    NumeroExterior = domicilio.CNumeroExterior,
                    NumeroInterior = domicilio.CNumeroInterior,
                    Colonia = domicilio.CColonia,
                    CodigoPostal = domicilio.CCodigoPostal,
                    Ciudad = domicilio.CCiudad,
                    Municipio = domicilio.CMunicipio,
                    Estado = domicilio.CEstado,
                    Email = domicilio.CEmail,
                    Pais = domicilio.CPais,
                    Telefono1 = domicilio.CTelefono1,
                    Telefono2 = domicilio.CTelefono2,
                    TelefonoCompleto = $"{domicilio.CTelefono1}{(!string.IsNullOrWhiteSpace(domicilio.CTelefono2) ? $" / {domicilio.CTelefono2}" : "")}"
                };
            }

            return dto;
        }



        /// <summary>
        /// Obtener primer teléfono disponible
        /// </summary>
        private string ObtenerTelefono(AdmCliente cliente, AdmDomicilio? domicilio)
        {
            // 1. Buscar en domicilio (prioridad)
            if (domicilio != null)
            {
                if (!string.IsNullOrWhiteSpace(domicilio.CTelefono1)) return domicilio.CTelefono1;
                if (!string.IsNullOrWhiteSpace(domicilio.CTelefono2)) return domicilio.CTelefono2;
                if (!string.IsNullOrWhiteSpace(domicilio.CTelefono3)) return domicilio.CTelefono3;
                if (!string.IsNullOrWhiteSpace(domicilio.CTelefono4)) return domicilio.CTelefono4;
            }

            // 2. Buscar en campos de contacto del cliente
            if (!string.IsNullOrWhiteSpace(cliente.CCon1Tel)) return cliente.CCon1Tel;
            if (!string.IsNullOrWhiteSpace(cliente.CWhatsapp)) return cliente.CWhatsapp;

            return "Sin teléfono";
        }

        /// <summary>
        /// Obtener primer email disponible
        /// </summary>
        private string ObtenerEmail(AdmCliente cliente, AdmDomicilio? domicilio)
        {
            // Prioridad: domicilio > cliente email1 > cliente email2
            if (domicilio != null && !string.IsNullOrWhiteSpace(domicilio.CEmail))
                return domicilio.CEmail;

            if (!string.IsNullOrWhiteSpace(cliente.CEmail1))
                return cliente.CEmail1;

            if (!string.IsNullOrWhiteSpace(cliente.CEmail2))
                return cliente.CEmail2;

            if (!string.IsNullOrWhiteSpace(cliente.CEmail3))
                return cliente.CEmail3;

            return "Sin email";
        }

        /// <summary>
        /// Construir cadena de ubicación formateada
        /// </summary>
        private string ConstruirUbicacion(AdmDomicilio? domicilio)
        {
            if (domicilio == null)
                return "Sin ubicación";

            var partes = new List<string>();

            // Calle y número
            if (!string.IsNullOrWhiteSpace(domicilio.CNombreCalle))
            {
                var direccion = domicilio.CNombreCalle.Trim();
                if (!string.IsNullOrWhiteSpace(domicilio.CNumeroExterior))
                    direccion += $" {domicilio.CNumeroExterior.Trim()}";
                partes.Add(direccion);
            }

            // Colonia
            if (!string.IsNullOrWhiteSpace(domicilio.CColonia))
                partes.Add(domicilio.CColonia.Trim());

            // Ciudad, Estado
            var ciudadEstado = new List<string>();
            if (!string.IsNullOrWhiteSpace(domicilio.CCiudad))
                ciudadEstado.Add(domicilio.CCiudad.Trim());
            if (!string.IsNullOrWhiteSpace(domicilio.CEstado))
                ciudadEstado.Add(domicilio.CEstado.Trim());

            if (ciudadEstado.Any())
                partes.Add(string.Join(", ", ciudadEstado));

            return partes.Any()
                ? string.Join("\n", partes)
                : "Sin ubicación";
        }
        /// <summary>
        /// Busca un cliente por RFC o email (para flujo de recuperación de contraseña)
        /// </summary>
        public async Task<AdmCliente?> BuscarPorRfcOEmailAsync(string? rfc, string? email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rfc) && string.IsNullOrWhiteSpace(email))
                    return null;

                return await _context.AdmClientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        (!string.IsNullOrEmpty(rfc) && c.CRfc.ToLower() == rfc.ToLower()) ||
                        (!string.IsNullOrEmpty(email) && (
                            c.CEmail1.ToLower() == email.ToLower() ||
                            c.CEmail2.ToLower() == email.ToLower() ||
                            c.CEmail3.ToLower() == email.ToLower()
                        ))
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar cliente por RFC/email");
                throw;
            }
        }

        /// <summary>
        /// Verificar existencia de un cliente por ID
        /// </summary>
        public async Task<bool> ExistsAsync(int idCliente)
        {
            return await _context.AdmClientes
                .AsNoTracking()
                .AnyAsync(c => c.CIdClienteProveedor == idCliente);
        }

        /// <summary>
        /// Verificar estatus activo del cliente
        /// </summary>
        public async Task<bool> IsActiveAsync(int? idCliente)
        {
            return await _context.AdmClientes
                .AsNoTracking()
                .AnyAsync(c => c.CIdClienteProveedor == idCliente && c.CEstatus == 1);
        }

        /// <summary>
        /// Lista paginada de clientes inactivos (CEstatus = 0), ordenados por fecha de alta más reciente
        /// OPTIMIZADO: Usa Redis con TTL corto (2 min) para reflejar cambios rápidos de activación
        /// </summary>
        public async Task<(List<AdmClienteConDomicilioResponseDto> Clientes, int TotalRegistros, int TotalPaginas)> GetClientesInactivosAsync(int numeroPagina = 1, int tamanoPagina = 50)
        {
            try
            {
                var cacheKey = $"AdmClientes:Inactivos:Pagina:{numeroPagina}:Tam:{tamanoPagina}";

                var cachedResult = await _cacheService.GetAsync<(List<AdmClienteConDomicilioResponseDto>, int, int)>(cacheKey);
                if (cachedResult != default)
                {
                    _logger.LogInformation("✅ Cache HIT - Clientes inactivos desde Redis. Key: {Key}, Count: {Count}",
                        cacheKey, cachedResult.Item1.Count);
                    return cachedResult;
                }

                _logger.LogInformation("🔍 Cache MISS - Consultando clientes inactivos en BD. Página: {Pagina}, Tamaño: {Tam}",
                    numeroPagina, tamanoPagina);

                var (clientes, total) = await _repository.GetClientesInactivosAsync(numeroPagina, tamanoPagina);

                var clientesConDomicilio = await CargarDomiciliosAsync(clientes, null, true);

                var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamanoPagina);

                var resultado = (clientesConDomicilio, total, totalPaginas);

                // TTL corto (2 min) porque el listado cambia con cada autorización
                await _cacheService.SetAsync(cacheKey, resultado, TimeSpan.FromMinutes(2));
                _logger.LogInformation("💾 Clientes inactivos guardados en Redis. Count: {Count}, Key: {Key}",
                    clientesConDomicilio.Count, cacheKey);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener clientes inactivos");
                throw;
            }
        }

        /// <summary>
        /// Detalle completo de un cliente inactivo para revisión previa a activación.
        /// Retorna null si el cliente no existe. Lanza InvalidOperationException si ya está activo.
        /// </summary>
        public async Task<AdmClienteConDomicilioResponseDto?> GetDetalleClienteInactivoAsync(int idCliente)
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo detalle de cliente inactivo ID: {IdCliente}", idCliente);

                var cliente = await _context.AdmClientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CIdClienteProveedor == idCliente);

                if (cliente == null)
                {
                    _logger.LogWarning("⚠️ Cliente {IdCliente} no encontrado", idCliente);
                    return null;
                }

                if (cliente.CEstatus == 1)
                {
                    _logger.LogWarning("⚠️ Cliente {IdCliente} ya se encuentra activo (CEstatus=1)", idCliente);
                    throw new InvalidOperationException($"El cliente con ID {idCliente} ya está activo y no requiere activación.");
                }

                var domicilios = await CargarDomiciliosAsync(new List<AdmCliente> { cliente }, null, true);

                _logger.LogInformation("✅ Detalle de cliente inactivo {IdCliente} obtenido", idCliente);

                return domicilios.FirstOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw; // Relanzan para que el controller devuelva 409 Conflict
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener detalle del cliente inactivo {IdCliente}", idCliente);
                throw;
            }
        }
    }
}
