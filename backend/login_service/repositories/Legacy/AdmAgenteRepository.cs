// =====================================================================================
// REPOSITORY ADM AGENTE - AdmAgenteRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa acceso a datos de agentes legacy (admAgentes) con Entity Framework Core.
// Proporciona consultas paginadas y búsquedas optimizadas.
//
// PROPÓSITO:
// - Acceso eficiente a BD legacy adCABS2016
// - Paginación con Skip/Take
// - Búsqueda por código y nombre
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
    /// Repositorio para acceso a agentes legacy (admAgentes de adCABS2016)
    /// Solo lectura con paginación optimizada
    /// </summary>
    public class AdmAgenteRepository : IAdmAgenteRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmAgenteRepository> _logger;

        public AdmAgenteRepository(LegacyCompacReadOnlyContext context, ILogger<AdmAgenteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL PAGINADO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todos los agentes con paginación
        /// Usa AsNoTracking para optimización (solo lectura)
        /// </summary>
        public async Task<(List<AdmAgente> Data, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogDebug("🔍 Consultando agentes legacy página {Page} tamaño {PageSize}", page, pageSize);

                // Contar total de registros
                var totalRecords = await _context.AdmAgentes.CountAsync();

                // Calcular skip
                var skip = (page - 1) * pageSize;

                // Obtener datos paginados
                var data = await _context.AdmAgentes
                    .AsNoTracking()
                    .OrderBy(a => a.CCodigoAgente)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("✅ Consulta exitosa: {Count} de {Total} agentes legacy (página {Page})",
                    data.Count, totalRecords, page);

                return (data, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consultar agentes legacy paginados (página {Page}, tamaño {PageSize})",
                    page, pageSize);
                throw;
            }
        }

        public async Task<AdmAgente?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug("🔍 Consultando agente legacy con ID {Id}", id);

                var agente = await _context.AdmAgentes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.CIdAgente == id);

                _logger.LogInformation("✅ Consulta exitosa: Agente legacy con ID {Id}", id);

                return agente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consultar agente legacy con ID {Id}", id);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: BÚSQUEDA PAGINADA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca agentes por código o nombre con paginación
        /// Búsqueda case-insensitive en CCODIGOAGENTE y CNOMBREAGENTE
        /// </summary>
        public async Task<(List<AdmAgente> Data, int TotalRecords)> SearchPaginatedAsync(
            string searchTerm,
            int page,
            int pageSize)
        {
            try
            {
                _logger.LogDebug("🔍 Buscando agentes legacy: '{SearchTerm}' página {Page} tamaño {PageSize}",
                    searchTerm, page, pageSize);

                var normalizedTerm = searchTerm.Trim().ToLower();

                // Query base con filtros
                var query = _context.AdmAgentes
                    .AsNoTracking()
                    .Where(a =>
                        a.CCodigoAgente.ToLower().Contains(normalizedTerm) ||
                        a.CNombreAgente.ToLower().Contains(normalizedTerm)
                    );

                // Contar total de registros filtrados
                var totalRecords = await query.CountAsync();

                // Calcular skip
                var skip = (page - 1) * pageSize;

                // Obtener datos paginados
                var data = await query
                    .OrderBy(a => a.CCodigoAgente)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("✅ Búsqueda exitosa: {Count} de {Total} agentes legacy encontrados ('{SearchTerm}', página {Page})",
                    data.Count, totalRecords, searchTerm, page);

                return (data, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar agentes legacy ('{SearchTerm}', página {Page}, tamaño {PageSize})",
                    searchTerm, page, pageSize);
                throw;
            }
        }
    }
}
