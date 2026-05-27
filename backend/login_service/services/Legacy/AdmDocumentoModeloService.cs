// =====================================================================================
// SERVICE ADM DOCUMENTO MODELO - AdmDocumentoModeloService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa lógica de negocio para modelos de documentos legacy sin cache Redis.
// Catálogo pequeño y estable que no requiere cache.
//
// PROPÓSITO:
// - Lógica de negocio para modelos de documentos legacy
// - Consulta directa sin cache (catálogo pequeño)
// - Búsqueda por descripción
// - Mapeo de entidades a DTOs
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio de lógica de negocio para modelos de documentos legacy sin cache
    /// Implementa consulta directa para admDocumentosModelo de adCABS2016
    /// </summary>
    public class AdmDocumentoModeloService : IAdmDocumentoModeloService
    {
        private readonly IAdmDocumentoModeloRepository _repository;
        private readonly ILogger<AdmDocumentoModeloService> _logger;

        public AdmDocumentoModeloService(
            IAdmDocumentoModeloRepository repository,
            ILogger<AdmDocumentoModeloService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL
        // ═══════════════════════════════════════════════════════════════

        public async Task<List<AdmDocumentoModeloResponseDto>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Consultando todos los modelos de documentos legacy");

                var documentosModelo = await _repository.GetAllAsync();
                var documentosModeloDto = documentosModelo.Select(MapToDto).ToList();

                _logger.LogInformation("✅ Consulta exitosa: {Count} modelos de documentos legacy obtenidos",
                    documentosModeloDto.Count);

                return documentosModeloDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener modelos de documentos legacy");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: BÚSQUEDA POR DESCRIPCIÓN
        // ═══════════════════════════════════════════════════════════════

        public async Task<List<AdmDocumentoModeloResponseDto>> SearchByDescripcionAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Término de búsqueda vacío, retornando lista vacía");
                    return new List<AdmDocumentoModeloResponseDto>();
                }

                _logger.LogInformation("🔍 Buscando modelos de documentos legacy por descripción: '{SearchTerm}'", searchTerm);

                var documentosModelo = await _repository.SearchByDescripcionAsync(searchTerm);
                var documentosModeloDto = documentosModelo.Select(MapToDto).ToList();

                _logger.LogInformation("✅ Búsqueda exitosa: {Count} modelos de documentos legacy encontrados ('{SearchTerm}')",
                    documentosModeloDto.Count, searchTerm);

                return documentosModeloDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar modelos de documentos legacy ('{SearchTerm}')", searchTerm);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MAPEO: ENTIDAD -> DTO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Mapea entidad AdmDocumentoModelo a DTO de respuesta
        /// </summary>
        private static AdmDocumentoModeloResponseDto MapToDto(AdmDocumentoModelo documentoModelo)
        {
            return new AdmDocumentoModeloResponseDto
            {
                IdDocumentoDe = documentoModelo.CIdDocumentoDe,
                Descripcion = documentoModelo.CDescripcion,
                Naturaleza = documentoModelo.CNaturaleza,
                AfectaExistencia = documentoModelo.CAfectaExistencia,
                Modulo = documentoModelo.CModulo,
                NoFolio = documentoModelo.CNoFolio,
                IdConceptoDoctoAsumido = documentoModelo.CIdConceptoDoctoAsumido,
                UsaCliente = documentoModelo.CUsaCliente,
                UsaProveedor = documentoModelo.CUsaProveedor,
                IdAsientoContable = documentoModelo.CIdAsientoContable
            };
        }
    }
}