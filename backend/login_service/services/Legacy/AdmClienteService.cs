using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

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
