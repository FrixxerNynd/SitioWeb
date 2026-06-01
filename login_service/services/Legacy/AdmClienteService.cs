using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using CRM.DTOs.Request;
using Microsoft.EntityFrameworkCore;
using back_cabs.CRM.DTOs.ServiceResponse;
using Microsoft.OpenApi.Any;

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
        private readonly ILogger<AdmClienteService> _logger;
        private readonly ICacheService _cacheService;

        // Tiempos de caché
        private readonly TimeSpan _cacheDurationBusqueda = TimeSpan.FromMinutes(10);

        public AdmClienteService(
            IAdmClienteRepository repository,
            LegacyCompacReadOnlyContext context,
            ILogger<AdmClienteService> logger,
            ICacheService cacheService)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Registro de clientes nuevos con domicilio
        /// </summary>
        public async Task<ServiceResult<AdmClienteConDomicilioResponseDto>> RegistrarAsync(UserClientRequestDto clientData)
        {
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
                CCodigoAlterno = "",

                //Datos de Contacto
                CEmail1 = clientData.Email,
                CEmail2 = clientData.Email2,
                CEmail3 = clientData.Email3,
                CWhatsapp = clientData.Telefono,

                //Campos Extra
                CTextoExtra1 = clientData.Contraseña,
                CTextoExtra2 = "Cliente registrado desde sitio web",
                CTextoExtra3 = "",
                CTextoExtra4 = "",
                CTextoExtra5 = "",
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

            _logger.LogInformation("Nuevo cliente registrado con ID {IdCliente} y RFC {RFC}", clienteInsertado.CIdClienteProveedor, nuevoCliente.CRfc);

            // Crear domicilio predeterminado para el cliente
            var nuevoDomicilio = new AdmDomicilio
            {
                CIdCatalogo = clienteInsertado.CIdClienteProveedor,
                CTipoCatalogo = 1, // Cliente
                CTipoDireccion = 1, // 1 Fiscal / 0 Normal
                CNombreCalle = clientData.UbicacionDetalle.Calle,
                CNumeroExterior = clientData.UbicacionDetalle.NumeroExterior,
                CNumeroInterior = clientData.UbicacionDetalle.NumeroInterior,
                CColonia = clientData.UbicacionDetalle.Colonia,
                CCodigoPostal = clientData.UbicacionDetalle.CodigoPostal,
                CCiudad = clientData.UbicacionDetalle.Ciudad,
                CMunicipio = clientData.UbicacionDetalle.Municipio,
                CEstado = clientData.UbicacionDetalle.Estado,
                CPais = clientData.UbicacionDetalle.Pais,

                //Datos de contacto del domicilio
                CTelefono1 = clientData.UbicacionDetalle.Telefono1,
                CTelefono2 = clientData.UbicacionDetalle.Telefono2,
                CTelefono3 = clientData.UbicacionDetalle.TelefonoCompleto,
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

        public async Task<AdmCliente?> ValidateCredentialsAsync(string email, string contrasena)
        {
            // Validar en el repositorio de clientes para soporte legacy
            var cliente = await _repository.ValidateCredentialsAsync(email, contrasena);
            if (cliente == null)
            {
                _logger.LogWarning("⚠️ Credenciales inválidas para email: {Email}", email);
                return null;
            }

            _logger.LogInformation("✅ Credenciales válidas para cliente ID: {IdCliente}, Email: {Email}", cliente.CIdClienteProveedor, email);
            return cliente;
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
    }
}
