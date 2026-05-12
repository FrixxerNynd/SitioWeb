// =====================================================================================
// REPOSITORY ADM MONEDA - AdmMonedaRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa acceso a datos de monedas legacy (admMonedas) con Entity Framework Core.
// Proporciona consultas paginadas optimizadas para catálogo pequeño.
//
// PROPÓSITO:
// - Acceso eficiente a BD legacy adCABS2016
// - Paginación con Skip/Take
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
    /// Repositorio para acceso a monedas legacy (admMonedas de adCABS2016)
    /// Solo lectura con paginación optimizada
    /// </summary>
    public class AdmMonedaRepository : IAdmMonedaRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmMonedaRepository> _logger;

        public AdmMonedaRepository(LegacyCompacReadOnlyContext context, ILogger<AdmMonedaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL PAGINADO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todas las monedas con paginación
        /// Usa AsNoTracking para optimización (solo lectura)
        /// </summary>
        public async Task<(List<AdmMoneda> Data, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogDebug("🔍 Consultando monedas legacy página {Page} tamaño {PageSize}", page, pageSize);

                // Contar total de registros
                var totalRecords = await _context.AdmMonedas.CountAsync();

                // Calcular skip
                var skip = (page - 1) * pageSize;

                // Obtener datos paginados
                var data = await _context.AdmMonedas
                    .AsNoTracking()
                    .OrderBy(m => m.CNombreMoneda)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("✅ Consulta exitosa: {Count} de {Total} monedas legacy (página {Page})",
                    data.Count, totalRecords, page);

                return (data, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consultar monedas legacy paginados (página {Page}, tamaño {PageSize})",
                    page, pageSize);
                throw;
            }
        }
    }
}