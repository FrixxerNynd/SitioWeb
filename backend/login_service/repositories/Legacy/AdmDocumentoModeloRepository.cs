// =====================================================================================
// REPOSITORY ADM DOCUMENTO MODELO - AdmDocumentoModeloRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa acceso a datos de modelos de documentos legacy (admDocumentosModelo) con Entity Framework Core.
// Proporciona consulta completa y búsqueda optimizadas para catálogo pequeño.
//
// PROPÓSITO:
// - Acceso eficiente a BD legacy adCABS2016
// - Consulta completa sin paginación (catálogo pequeño ~38 registros)
// - Búsqueda por descripción
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
    /// Repositorio para acceso a modelos de documentos legacy (admDocumentosModelo de adCABS2016)
    /// Consulta completa sin paginación para catálogo pequeño
    /// </summary>
    public class AdmDocumentoModeloRepository : IAdmDocumentoModeloRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmDocumentoModeloRepository> _logger;

        public AdmDocumentoModeloRepository(LegacyCompacReadOnlyContext context, ILogger<AdmDocumentoModeloRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todos los modelos de documentos sin paginación
        /// Usa AsNoTracking para optimización (solo lectura)
        /// </summary>
        public async Task<List<AdmDocumentoModelo>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("🔍 Consultando todos los modelos de documentos legacy");

                var documentosModelo = await _context.AdmDocumentosModelo
                    .AsNoTracking()
                    .OrderBy(d => d.CDescripcion)
                    .ToListAsync();

                _logger.LogInformation("✅ Consulta exitosa: {Count} modelos de documentos legacy obtenidos",
                    documentosModelo.Count);

                return documentosModelo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consultar modelos de documentos legacy");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: BÚSQUEDA POR DESCRIPCIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca modelos de documentos por descripción
        /// Búsqueda case-insensitive en CDESCRIPCION
        /// </summary>
        public async Task<List<AdmDocumentoModelo>> SearchByDescripcionAsync(string searchTerm)
        {
            try
            {
                _logger.LogDebug("🔍 Buscando modelos de documentos legacy: '{SearchTerm}'", searchTerm);

                var normalizedTerm = searchTerm.Trim().ToLower();

                var documentosModelo = await _context.AdmDocumentosModelo
                    .AsNoTracking()
                    .Where(d => d.CDescripcion.ToLower().Contains(normalizedTerm))
                    .OrderBy(d => d.CDescripcion)
                    .ToListAsync();

                _logger.LogInformation("✅ Búsqueda exitosa: {Count} modelos de documentos legacy encontrados ('{SearchTerm}')",
                    documentosModelo.Count, searchTerm);

                return documentosModelo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar modelos de documentos legacy ('{SearchTerm}')", searchTerm);
                throw;
            }
        }
    }
}