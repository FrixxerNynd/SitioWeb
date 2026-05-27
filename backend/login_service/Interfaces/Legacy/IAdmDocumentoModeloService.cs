// =====================================================================================
// INTERFAZ SERVICE ADM DOCUMENTO MODELO - IAdmDocumentoModeloService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de servicios de negocio para modelos de documentos legacy.
// Sin cache Redis ya que es catálogo pequeño y estable.
//
// PROPÓSITO:
// - Lógica de negocio para modelos de documentos legacy
// - Consulta directa sin cache (catálogo pequeño)
// - Búsqueda por descripción
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para servicio de modelos de documentos legacy sin cache
    /// Proporciona operaciones de negocio para admDocumentosModelo de adCABS2016
    /// </summary>
    public interface IAdmDocumentoModeloService
    {
        /// <summary>
        /// Obtiene todos los modelos de documentos sin paginación ni cache
        /// </summary>
        /// <returns>Lista completa de modelos de documentos como DTOs</returns>
        Task<List<AdmDocumentoModeloResponseDto>> GetAllAsync();

        /// <summary>
        /// Busca modelos de documentos por descripción sin cache
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (descripción)</param>
        /// <returns>Lista de modelos de documentos filtrados como DTOs</returns>
        Task<List<AdmDocumentoModeloResponseDto>> SearchByDescripcionAsync(string searchTerm);
    }
}