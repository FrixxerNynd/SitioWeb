// =====================================================================================
// REPOSITORY ADM CONCEPTO - AdmConceptoRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa acceso a datos de conceptos legacy (admConceptos) con Entity Framework Core.
// Proporciona consultas paginadas y búsquedas optimizadas por código/nombre.
//
// PROPÓSITO:
// - Acceso eficiente a BD legacy adCABS2016
// - Paginación con Skip/Take (50 registros por página)
// - Búsqueda por código y nombre de concepto
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
    /// Repositorio para acceso a conceptos legacy (admConceptos de adCABS2016)
    /// Solo lectura con paginación optimizada
    /// </summary>
    public class AdmConceptoRepository : IAdmConceptoRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmConceptoRepository> _logger;

        public AdmConceptoRepository(LegacyCompacReadOnlyContext context, ILogger<AdmConceptoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL PAGINADO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todos los conceptos con paginación
        /// Usa AsNoTracking para optimización (solo lectura)
        /// </summary>
        public async Task<(List<AdmConcepto> Data, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogDebug("🔍 Consultando conceptos legacy página {Page} tamaño {PageSize}", page, pageSize);

                // Contar total de registros
                var totalRecords = await _context.AdmConceptos.CountAsync();

                // Calcular skip
                var skip = (page - 1) * pageSize;

                // Obtener datos paginados
                var data = await _context.AdmConceptos
                    .AsNoTracking()
                    .OrderBy(c => c.CCodigoConcepto)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("✅ Consulta exitosa: {Count} de {Total} conceptos legacy (página {Page})",
                    data.Count, totalRecords, page);

                return (data, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consultar conceptos legacy paginados (página {Page}, tamaño {PageSize})",
                    page, pageSize);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: BÚSQUEDA PAGINADA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca conceptos por código o nombre con paginación
        /// Búsqueda case-insensitive en CCODIGOCONCEPTO y CNOMBRECONCEPTO
        /// </summary>
        public async Task<(List<AdmConcepto> Data, int TotalRecords)> SearchPaginatedAsync(
            string searchTerm,
            int page,
            int pageSize)
        {
            try
            {
                _logger.LogDebug("🔍 Buscando conceptos legacy: '{SearchTerm}' página {Page} tamaño {PageSize}",
                    searchTerm, page, pageSize);

                var normalizedTerm = searchTerm.Trim().ToLower();

                // Query base con filtros
                var query = _context.AdmConceptos
                    .AsNoTracking()
                    .Where(c =>
                        c.CCodigoConcepto.ToLower().Contains(normalizedTerm) ||
                        c.CNombreConcepto.ToLower().Contains(normalizedTerm)
                    );

                // Contar total de registros filtrados
                var totalRecords = await query.CountAsync();

                // Calcular skip
                var skip = (page - 1) * pageSize;

                // Obtener datos paginados
                var data = await query
                    .OrderBy(c => c.CCodigoConcepto)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("✅ Búsqueda exitosa: {Count} de {Total} conceptos legacy encontrados ('{SearchTerm}', página {Page})",
                    data.Count, totalRecords, searchTerm, page);

                return (data, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar conceptos legacy ('{SearchTerm}', página {Page}, tamaño {PageSize})",
                    searchTerm, page, pageSize);
                throw;
            }
        }
    }
}