// =====================================================================================
// REPOSITORY ADM PRODUCTO - AdmProductoRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa acceso a datos de productos legacy (admProductos) con Entity Framework Core.
// Proporciona consultas paginadas y búsquedas optimizadas por código/nombre.
//
// PROPÓSITO:
// - Acceso eficiente a BD legacy adCABS2016
// - Paginación con Skip/Take (50 registros por página)
// - Búsqueda por código y nombre de producto
// - AsNoTracking para optimización de solo lectura
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para acceso a productos legacy (admProductos de adCABS2016)
    /// Solo lectura con paginación optimizada
    /// </summary>
    public class AdmProductoRepository : IAdmProductoRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmProductoRepository> _logger;

        public AdmProductoRepository(LegacyCompacReadOnlyContext context, ILogger<AdmProductoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL PAGINADO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todos los productos con paginación
        /// Usa AsNoTracking para optimización (solo lectura)
        /// </summary>
        public async Task<(List<AdmProducto> Data, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize, int? status = null)
        {
            try
            {
                _logger.LogDebug("🔍 Consultando productos legacy página {Page} tamaño {PageSize} estado {Status}", page, pageSize, status);

                // Query base
                var query = _context.AdmProductos.AsNoTracking();

                // Aplicar filtro de estado si se proporciona
                if (status.HasValue)
                {
                    query = query.Where(p => p.CStatusProducto == status.Value);
                }

                // Contar total de registros
                var totalRecords = await query.CountAsync();

                // Calcular skip
                var skip = (page - 1) * pageSize;

                // Obtener datos paginados
                var data = await query
                    .OrderBy(p => p.CCodigoProducto)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("✅ Consulta exitosa: {Count} de {Total} productos legacy (página {Page})",
                    data.Count, totalRecords, page);

                return (data, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consultar productos legacy paginados (página {Page}, tamaño {PageSize})",
                    page, pageSize);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: BÚSQUEDA PAGINADA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca productos por código o nombre sin paginación
        /// Búsqueda case-insensitive en CCODIGOPRODUCTO y CNOMBREPRODUCTO
        /// </summary>
        public async Task<List<AdmProducto>> SearchAsync(
            string searchTerm,
            int? status = null)
        {
            try
            {
                _logger.LogDebug("🔍 Buscando productos legacy: '{SearchTerm}' estado {Status}",
                    searchTerm, status);

                var normalizedTerm = searchTerm.Trim().ToLower();

                // Query base con filtros
                var query = _context.AdmProductos.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(normalizedTerm))
                {
                    query = query.Where(p => 
                        p.CCodigoProducto.ToLower().Contains(normalizedTerm) || 
                        p.CNombreProducto.ToLower().Contains(normalizedTerm));
                }

                if (status.HasValue)
                {
                    query = query.Where(p => p.CStatusProducto == status.Value);
                }

                // Obtener datos completos
                var data = await query
                    .OrderBy(p => p.CCodigoProducto)
                    .ToListAsync();

                _logger.LogInformation("✅ Búsqueda exitosa: {Count} productos legacy encontrados",
                    data.Count);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar productos legacy ('{SearchTerm}')",
                    searchTerm);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET BY ID
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene un producto por su ID
        /// </summary>
        public async Task<AdmProducto?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug("🔍 Buscando producto legacy por ID: {Id}", id);

                var producto = await _context.AdmProductos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.CIdProducto == id);

                if (producto != null)
                {
                    _logger.LogInformation("✅ Producto legacy encontrado: {Id} - {Nombre}", id, producto.CNombreProducto);
                }
                else
                {
                    _logger.LogWarning("⚠️ Producto legacy no encontrado: {Id}", id);
                }

                return producto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener producto legacy por ID: {Id}", id);
                throw;
            }
        }
    }
}