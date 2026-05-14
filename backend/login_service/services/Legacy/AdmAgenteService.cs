// =====================================================================================
// SERVICE ADM AGENTE - AdmAgenteService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa lógica de negocio para agentes legacy con cache Redis.
// Usa Cache-Aside pattern para optimizar consultas a admAgentes.
//
// PROPÓSITO:
// - Lógica de negocio para agentes legacy
// - Cache Redis con TTL 30min (GetAll) y 60min (Search)
// - Mapeo de entidades a DTOs
// - Validación de parámetros de paginación
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using back_cabs.CRM.services;
using CRM.DTOs.Response;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio de lógica de negocio para agentes legacy con cache Redis
    /// Implementa Cache-Aside pattern para admAgentes de adCABS2016
    /// </summary>
    public class AdmAgenteService : IAdmAgenteService
    {
        private readonly IAdmAgenteRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AdmAgenteService> _logger;

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN DE CACHE
        // ═══════════════════════════════════════════════════════════════

        private const string CACHE_KEY_ALL = "adm_agentes:all:page:{0}:size:{1}";
        private const string CACHE_KEY_SEARCH = "adm_agentes:search:{0}:page:{1}:size:{2}";
        private static readonly TimeSpan CACHE_TTL_GET_ALL = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan CACHE_TTL_SEARCH = TimeSpan.FromMinutes(60);

        public AdmAgenteService(
            IAdmAgenteRepository repository,
            ICacheService cacheService,
            ILogger<AdmAgenteService> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL PAGINADO CON CACHE
        // ═══════════════════════════════════════════════════════════════

        public async Task<PaginatedResponseDto<AdmAgenteResponseDto>> GetAllPaginatedAsync(int page, int pageSize)
        {
            try
            {
                // Validación de parámetros
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 30;
                if (pageSize > 100) pageSize = 100;

                var cacheKey = string.Format(CACHE_KEY_ALL, page, pageSize);

                // ✅ PASO 1: Verificar cache
                _logger.LogDebug("🔍 Verificando cache Redis con key: {CacheKey}", cacheKey);

                var cachedData = await _cacheService.GetAsync<PaginatedResponseDto<AdmAgenteResponseDto>>(cacheKey);

                if (cachedData != null)
                {
                    _logger.LogInformation("✅ CACHE HIT página {Page} - Retornando {Count} de {TotalRecords} agentes legacy desde Redis",
                        page, cachedData.Items.Count(), cachedData.TotalItems);
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS página {Page} tamaño {PageSize} - Consultando base de datos legacy",
                    page, pageSize);

                var (data, totalRecords) = await _repository.GetAllPaginatedAsync(page, pageSize);
                var agentesDto = data.Select(MapToDto).ToList();

                // Crear respuesta paginada
                var paginatedResponse = new PaginatedResponseDto<AdmAgenteResponseDto>
                {
                    Items = agentesDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };

                // ✅ PASO 3: Guardar en cache
                await _cacheService.SetAsync(cacheKey, paginatedResponse, CACHE_TTL_GET_ALL);
                _logger.LogInformation("💾 Cache SET para página {Page} con {Count} de {TotalRecords} agentes legacy. TTL: {TTL} minutos",
                    page, agentesDto.Count, totalRecords, CACHE_TTL_GET_ALL.TotalMinutes);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener agentes legacy paginados (Página {Page}, Tamaño {PageSize}). Fallback a BD sin cache.",
                    page, pageSize);

                // Fallback sin cache
                var (data, totalRecords) = await _repository.GetAllPaginatedAsync(page, pageSize);
                var agentesDto = data.Select(MapToDto).ToList();
                return new PaginatedResponseDto<AdmAgenteResponseDto>
                {
                    Items = agentesDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: BÚSQUEDA PAGINADA CON CACHE
        // ═══════════════════════════════════════════════════════════════

        public async Task<PaginatedResponseDto<AdmAgenteResponseDto>> SearchPaginatedAsync(
            string searchTerm,
            int page,
            int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Término de búsqueda vacío, retornando lista vacía");
                    return new PaginatedResponseDto<AdmAgenteResponseDto>
                    {
                        Items = new List<AdmAgenteResponseDto>(),
                        Pagina = page,
                        ResultadosPorPagina = pageSize,
                        TotalItems = 0
                    };
                }

                // Validación de parámetros
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 30;
                if (pageSize > 100) pageSize = 100;

                var normalizedTerm = searchTerm.Trim().ToLower();
                var cacheKey = string.Format(CACHE_KEY_SEARCH, normalizedTerm, page, pageSize);

                // ✅ PASO 1: Verificar cache
                _logger.LogDebug("🔍 Verificando cache Redis búsqueda con key: {CacheKey}", cacheKey);

                var cachedData = await _cacheService.GetAsync<PaginatedResponseDto<AdmAgenteResponseDto>>(cacheKey);

                if (cachedData != null)
                {
                    _logger.LogInformation("✅ CACHE HIT búsqueda '{SearchTerm}' página {Page} - Retornando {Count} de {TotalRecords} agentes legacy desde Redis",
                        searchTerm, page, cachedData.Items.Count(), cachedData.TotalItems);
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS búsqueda '{SearchTerm}' página {Page} tamaño {PageSize} - Consultando base de datos legacy",
                    searchTerm, page, pageSize);

                var (data, totalRecords) = await _repository.SearchPaginatedAsync(searchTerm, page, pageSize);
                var agentesDto = data.Select(MapToDto).ToList();

                // Crear respuesta paginada
                var paginatedResponse = new PaginatedResponseDto<AdmAgenteResponseDto>
                {
                    Items = agentesDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };

                // ✅ PASO 3: Guardar en cache
                await _cacheService.SetAsync(cacheKey, paginatedResponse, CACHE_TTL_SEARCH);
                _logger.LogInformation("💾 Cache SET búsqueda '{SearchTerm}' página {Page} con {Count} de {TotalRecords} agentes legacy. TTL: {TTL} minutos",
                    searchTerm, page, agentesDto.Count, totalRecords, CACHE_TTL_SEARCH.TotalMinutes);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar agentes legacy ('{SearchTerm}', Página {Page}, Tamaño {PageSize}). Fallback a BD sin cache.",
                    searchTerm, page, pageSize);

                // Fallback sin cache
                var (data, totalRecords) = await _repository.SearchPaginatedAsync(searchTerm, page, pageSize);
                var agentesDto = data.Select(MapToDto).ToList();
                return new PaginatedResponseDto<AdmAgenteResponseDto>
                {
                    Items = agentesDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MAPEO: ENTIDAD -> DTO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Mapea entidad AdmAgente a DTO de respuesta
        /// </summary>
        private static AdmAgenteResponseDto MapToDto(AdmAgente agente)
        {
            return new AdmAgenteResponseDto
            {
                IdAgente = agente.CIdAgente,
                CodigoAgente = agente.CCodigoAgente,
                NombreAgente = agente.CNombreAgente,
                FechaAlta = agente.CFechaAltaAgente,
                TipoAgente = agente.CTipoAgente,
                ComisionVenta = agente.CComisionVentaAgente,
                ComisionCobro = agente.CComisionCobroAgente,
                ClienteId = agente.CIdCliente,
                ProveedorId = agente.CIdProveedor,
                Clasificacion1 = agente.CIdValorClasificacion1,
                Clasificacion2 = agente.CIdValorClasificacion2,
                Clasificacion3 = agente.CIdValorClasificacion3,
                TextoExtra2 = agente.CTextoExtra2,
                Timestamp = agente.CTimestamp
            };
        }
    }
}
