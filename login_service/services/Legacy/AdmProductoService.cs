// =====================================================================================
// SERVICE ADM PRODUCTO - AdmProductoService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa lógica de negocio para productos legacy con cache Redis robusto.
// Usa Cache-Aside pattern para optimizar consultas a admProductos.
//
// PROPÓSITO:
// - Lógica de negocio para productos legacy
// - Cache Redis con TTL 30min (GetAll) y 60min (Search)
// - Mapeo de entidades a DTOs optimizado
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
    /// Servicio de lógica de negocio para productos legacy con cache Redis
    /// Implementa Cache-Aside pattern para admProductos de adCABS2016
    /// </summary>
    public class AdmProductoService : IAdmProductoService
    {
        private readonly IAdmProductoRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AdmProductoService> _logger;

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN DE CACHE
        // ═══════════════════════════════════════════════════════════════

        private const string CACHE_KEY_ALL = "adm_productos:all:page:{0}:size:{1}:status:{2}";
        private const string CACHE_KEY_SEARCH = "adm_productos:search:{0}:page:{1}:size:{2}:status:{3}";
        private const string CACHE_KEY_ID = "adm_productos:id:{0}";
        private static readonly TimeSpan CACHE_TTL_GET_ALL = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan CACHE_TTL_SEARCH = TimeSpan.FromMinutes(60);
        private static readonly TimeSpan CACHE_TTL_ID = TimeSpan.FromMinutes(30);

        public AdmProductoService(
            IAdmProductoRepository repository,
            ICacheService cacheService,
            ILogger<AdmProductoService> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL PAGINADO CON CACHE
        // ═══════════════════════════════════════════════════════════════

        public async Task<PaginatedResponseDto<AdmProductoResponseDto>> GetAllPaginatedAsync(int page, int pageSize, int? status = null)
        {
            try
            {
                // Validación de parámetros
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 50;
                if (pageSize > 100) pageSize = 100;

                var statusKey = status.HasValue ? status.Value.ToString() : "all";
                var cacheKey = string.Format(CACHE_KEY_ALL, page, pageSize, statusKey);

                // ✅ PASO 1: Verificar cache
                _logger.LogDebug("🔍 Verificando cache Redis con key: {CacheKey}", cacheKey);

                var cachedData = await _cacheService.GetAsync<PaginatedResponseDto<AdmProductoResponseDto>>(cacheKey);

                if (cachedData != null)
                {
                    _logger.LogInformation("✅ CACHE HIT página {Page} - Retornando {Count} de {TotalRecords} productos legacy desde Redis",
                        page, cachedData.Items.Count(), cachedData.TotalItems);
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS página {Page} tamaño {PageSize} estado {Status} - Consultando base de datos legacy",
                    page, pageSize, status);

                var (data, totalRecords) = await _repository.GetAllPaginatedAsync(page, pageSize, status);
                var productosDto = data.Select(MapToDto).ToList();

                // Crear respuesta paginada
                var paginatedResponse = new PaginatedResponseDto<AdmProductoResponseDto>
                {
                    Items = productosDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };

                // ✅ PASO 3: Guardar en cache
                await _cacheService.SetAsync(cacheKey, paginatedResponse, CACHE_TTL_GET_ALL);
                _logger.LogInformation("💾 Cache SET para página {Page} con {Count} de {TotalRecords} productos legacy. TTL: {TTL} minutos",
                    page, productosDto.Count, totalRecords, CACHE_TTL_GET_ALL.TotalMinutes);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener productos legacy paginados (Página {Page}, Tamaño {PageSize}). Fallback a BD sin cache.",
                    page, pageSize);

                // Fallback sin cache
                var (data, totalRecords) = await _repository.GetAllPaginatedAsync(page, pageSize, status);
                var productosDto = data.Select(MapToDto).ToList();
                return new PaginatedResponseDto<AdmProductoResponseDto>
                {
                    Items = productosDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };
            }
        }
        
        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: BÚSQUEDA SIN PAGINACIÓN (DEVUELVE TODO) CON CACHE
        // ═══════════════════════════════════════════════════════════════

        public async Task<PaginatedResponseDto<AdmProductoResponseDto>> SearchPaginatedAsync(
            string searchTerm,
            int page,      // Se mantiene por compatibilidad, pero se ignora
            int pageSize,  // Se mantiene por compatibilidad, pero se ignora
            int? status = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Término de búsqueda vacío, retornando lista vacía");
                    return new PaginatedResponseDto<AdmProductoResponseDto>
                    {
                        Items = new List<AdmProductoResponseDto>(),
                        Pagina = 1,
                        ResultadosPorPagina = 0,
                        TotalItems = 0
                    };
                }

                var normalizedTerm = searchTerm.Trim().ToLower();
                var statusKey = status.HasValue ? status.Value.ToString() : "all";

                // CAMBIO 1: La key del cache ya NO depende de la página ni del tamaño.
                // Usamos "ALL" o 0 para indicar que este cache contiene TODO el dataset de esa búsqueda.
                // Asumo que tu constante CACHE_KEY_SEARCH espera 4 argumentos.
                var cacheKey = string.Format(CACHE_KEY_SEARCH, normalizedTerm, "ALL", "ALL", statusKey);

                // ✅ PASO 1: Verificar cache
                _logger.LogDebug("🔍 Verificando cache Redis búsqueda completa con key: {CacheKey}", cacheKey);

                var cachedData = await _cacheService.GetAsync<PaginatedResponseDto<AdmProductoResponseDto>>(cacheKey);

                if (cachedData != null)
                {
                    _logger.LogInformation("✅ CACHE HIT búsqueda '{SearchTerm}' - Retornando todos los {Count} productos desde Redis",
                        searchTerm, cachedData.Items.Count());
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS búsqueda '{SearchTerm}' - Consultando base de datos legacy para obtener TODOS los registros",
                    searchTerm);

                // Obtenemos todos los datos del repositorio
                var allData = await _repository.SearchAsync(searchTerm, status);
                
                // Mapeamos a DTO
                var productosDto = allData.Select(MapToDto).ToList();
                var totalRecords = productosDto.Count;

                // Crear respuesta (sin recortar la lista)
                var response = new PaginatedResponseDto<AdmProductoResponseDto>
                {
                    Items = productosDto,       // Enviamos la lista completa
                    Pagina = 1,                 // Siempre será la página 1
                    ResultadosPorPagina = totalRecords, // El tamaño de página es el total de registros
                    TotalItems = totalRecords
                };

                // ✅ PASO 3: Guardar en cache
                await _cacheService.SetAsync(cacheKey, response, CACHE_TTL_SEARCH);
                _logger.LogInformation("💾 Cache SET búsqueda '{SearchTerm}' con {Count} productos (FULL LIST). TTL: {TTL} minutos",
                    searchTerm, totalRecords, CACHE_TTL_SEARCH.TotalMinutes);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar productos legacy ('{SearchTerm}'). Fallback a BD sin cache.", searchTerm);

                // Fallback sin cache
                var allData = await _repository.SearchAsync(searchTerm, status);
                var productosDto = allData.Select(MapToDto).ToList();
                
                return new PaginatedResponseDto<AdmProductoResponseDto>
                {
                    Items = productosDto,
                    Pagina = 1,
                    ResultadosPorPagina = productosDto.Count,
                    TotalItems = productosDto.Count
                };
            }
        }



        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: OBTENER POR ID CON CACHE
        // ═══════════════════════════════════════════════════════════════

        public async Task<AdmProductoResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                var cacheKey = string.Format(CACHE_KEY_ID, id);

                // ✅ PASO 1: Verificar cache
                _logger.LogDebug("🔍 Verificando cache Redis ID con key: {CacheKey}", cacheKey);

                var cachedData = await _cacheService.GetAsync<AdmProductoResponseDto>(cacheKey);

                if (cachedData != null)
                {
                    _logger.LogInformation("✅ CACHE HIT ID {Id} - Retornando producto desde Redis", id);
                    return cachedData;
                }

                // ✅ PASO 2: CACHE MISS - Consultar base de datos
                _logger.LogInformation("⚠️ CACHE MISS ID {Id} - Consultando base de datos legacy", id);

                var producto = await _repository.GetByIdAsync(id);

                if (producto == null)
                {
                    _logger.LogWarning("⚠️ Producto con ID {Id} no encontrado en BD", id);
                    return null;
                }

                var productoDto = MapToDto(producto);

                // ✅ PASO 3: Guardar en cache
                await _cacheService.SetAsync(cacheKey, productoDto, CACHE_TTL_ID);
                _logger.LogInformation("💾 Cache SET ID {Id}. TTL: {TTL} minutos",
                    id, CACHE_TTL_ID.TotalMinutes);

                return productoDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener producto por ID {Id}. Fallback a BD sin cache.", id);

                // Fallback sin cache
                var producto = await _repository.GetByIdAsync(id);
                return producto != null ? MapToDto(producto) : null;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MAPEO: ENTIDAD -> DTO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Mapea entidad AdmProducto a DTO de respuesta
        /// Solo incluye campos principales para optimizar performance
        /// </summary>
        private static AdmProductoResponseDto MapToDto(AdmProducto producto)
        {
            return new AdmProductoResponseDto
            {
                IdProducto = producto.CIdProducto,
                CodigoProducto = producto.CCodigoProducto,
                NombreProducto = producto.CNombreProducto,
                CodigoAlternativo = producto.CCodAltern,
                NombreAlternativo = producto.CNomAltern,
                DescripcionCorta = producto.CDescCorta,
                Descripcion = producto.CDescripcionProducto,
                TipoProducto = producto.CTipoProducto,
                FechaAlta = producto.CFechaAltaProducto,
                Status = producto.CStatusProducto,
                ControlExistencia = producto.CControlExistencia,
                Peso = producto.CPesoProducto,
                Precio1 = producto.CPrecio1,
                Precio2 = producto.CPrecio2,
                Precio3 = producto.CPrecio3,
                Precio4 = producto.CPrecio4,
                Precio5 = producto.CPrecio5,
                Precio6 = producto.CPrecio6,
                Precio7 = producto.CPrecio7,
                Precio8 = producto.CPrecio8,
                Precio9 = producto.CPrecio9,
                Precio10 = producto.CPrecio10,
                CostoEstandar = producto.CCostoEstandar,
                MetodoCosteo = producto.CMetodoCosteo,
                MargenUtilidad = producto.CMargenUtilidad,
                Impuesto1 = producto.CImpuesto1,
                Impuesto2 = producto.CImpuesto2,
                Impuesto3 = producto.CImpuesto3,
                Retencion1 = producto.CRetencion1,
                Retencion2 = producto.CRetencion2,
                EsExento = producto.CEsExento,
                IdUnidadBase = producto.CIdUnidadBase,
                IdUnidadNoConvertible = producto.CIdUnidadNoConvertible,
                IdUnidadCompra = producto.CIdUnidadCompra,
                IdUnidadVenta = producto.CIdUnidadVenta,
                IdMoneda = producto.CIdMoneda,
                Clasificacion1 = producto.CIdValorClasificacion1,
                Clasificacion2 = producto.CIdValorClasificacion2,
                Clasificacion3 = producto.CIdValorClasificacion3,
                Clasificacion4 = producto.CIdValorClasificacion4,
                Clasificacion5 = producto.CIdValorClasificacion5,
                Clasificacion6 = producto.CIdValorClasificacion6,
                ClaveSat = producto.CClaveSat,
                ExistenciaNegativa = producto.CExistenciaNegativa,
                Timestamp = producto.CTimestamp
            };
        }
    }
}