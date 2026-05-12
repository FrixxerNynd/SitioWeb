// =====================================================================================
// SERVICE ADM MONEDA - AdmMonedaService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa lógica de negocio para monedas legacy con cache Redis.
// Usa Cache-Aside pattern para optimizar consultas a admMonedas.
//
// PROPÓSITO:
// - Lógica de negocio para monedas legacy
// - Cache Redis con TTL 60min (catálogo pequeño y estable)
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
    /// Servicio de lógica de negocio para monedas legacy con cache Redis
    /// Implementa Cache-Aside pattern para admMonedas de adCABS2016
    /// </summary>
    public class AdmMonedaService : IAdmMonedaService
    {
        private readonly IAdmMonedaRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AdmMonedaService> _logger;

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN DE CACHE
        // ═══════════════════════════════════════════════════════════════

        private const string CACHE_KEY_ALL = "adm_monedas:all:page:{0}:size:{1}";
        private static readonly TimeSpan CACHE_TTL_GET_ALL = TimeSpan.FromMinutes(60); // Más tiempo ya que es catálogo estable

        public AdmMonedaService(
            IAdmMonedaRepository repository,
            ICacheService cacheService,
            ILogger<AdmMonedaService> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL PAGINADO CON CACHE
        // ═══════════════════════════════════════════════════════════════

        public async Task<PaginatedResponseDto<AdmMonedaResponseDto>> GetAllPaginatedAsync(int page, int pageSize)
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

                var cachedData = await _cacheService.GetAsync<PaginatedResponseDto<AdmMonedaResponseDto>>(cacheKey);

                if (cachedData != null)
                {
                    _logger.LogInformation("✅ CACHE HIT página {Page} - Retornando {Count} de {TotalRecords} monedas legacy desde Redis",
                        page, cachedData.Items.Count(), cachedData.TotalItems);
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS página {Page} tamaño {PageSize} - Consultando base de datos legacy",
                    page, pageSize);

                var (data, totalRecords) = await _repository.GetAllPaginatedAsync(page, pageSize);
                var monedasDto = data.Select(MapToDto).ToList();

                // Crear respuesta paginada
                var paginatedResponse = new PaginatedResponseDto<AdmMonedaResponseDto>
                {
                    Items = monedasDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };

                // ✅ PASO 3: Guardar en cache
                await _cacheService.SetAsync(cacheKey, paginatedResponse, CACHE_TTL_GET_ALL);
                _logger.LogInformation("💾 Cache SET para página {Page} con {Count} de {TotalRecords} monedas legacy. TTL: {TTL} minutos",
                    page, monedasDto.Count, totalRecords, CACHE_TTL_GET_ALL.TotalMinutes);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener monedas legacy paginados (Página {Page}, Tamaño {PageSize}). Fallback a BD sin cache.",
                    page, pageSize);

                // Fallback sin cache
                var (data, totalRecords) = await _repository.GetAllPaginatedAsync(page, pageSize);
                var monedasDto = data.Select(MapToDto).ToList();
                return new PaginatedResponseDto<AdmMonedaResponseDto>
                {
                    Items = monedasDto,
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
        /// Mapea entidad AdmMoneda a DTO de respuesta
        /// </summary>
        private static AdmMonedaResponseDto MapToDto(AdmMoneda moneda)
        {
            return new AdmMonedaResponseDto
            {
                IdMoneda = moneda.CIdMoneda,
                NombreMoneda = moneda.CNombreMoneda,
                SimboloMoneda = moneda.CSimboloMoneda,
                PosicionSimbolo = moneda.CPosicionSimbolo,
                Plural = moneda.CPlural,
                Singular = moneda.CSingular,
                DescripcionProtegida = moneda.CDescripcionProtegida,
                IdBandera = moneda.CIdBandera,
                DecimalesMoneda = moneda.CDecimalesMoneda,
                Timestamp = moneda.CTimestamp,
                ClaveSat = moneda.CClaveSat
            };
        }
    }
}