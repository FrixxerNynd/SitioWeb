// =====================================================================================
// REPOSITORY ADM ALMACEN - AdmAlmacenRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa acceso a datos de almacenes legacy (admAlmacenes) con Entity Framework Core.
// Proporciona consulta completa optimizada para catálogo pequeño.
//
// PROPÓSITO:
// - Acceso eficiente a BD legacy adCABS2016
// - Consulta completa sin paginación (catálogo pequeño)
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
    /// Repositorio para acceso a almacenes legacy (admAlmacenes de adCABS2016)
    /// Consulta completa sin paginación para catálogo pequeño
    /// </summary>
    public class AdmAlmacenRepository : IAdmAlmacenRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmAlmacenRepository> _logger;

        public AdmAlmacenRepository(LegacyCompacReadOnlyContext context, ILogger<AdmAlmacenRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todos los almacenes sin paginación
        /// Usa AsNoTracking para optimización (solo lectura)
        /// </summary>
        public async Task<List<AdmAlmacen>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("🔍 Consultando todos los almacenes legacy");

                var almacenes = await _context.AdmAlmacenes
                    .AsNoTracking()
                    .OrderBy(a => a.CCodigoAlmacen)
                    .ToListAsync();

                _logger.LogInformation("✅ Consulta exitosa: {Count} almacenes legacy obtenidos",
                    almacenes.Count);

                return almacenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consultar almacenes legacy");
                throw;
            }
        }
    }
}