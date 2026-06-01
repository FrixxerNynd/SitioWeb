// =====================================================================================
// INTERFAZ REPOSITORY ADM DOCUMENTO MODELO - IAdmDocumentoModeloRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para acceso a datos de modelos de documentos legacy (admDocumentosModelo).
// Especifica métodos para consulta completa y búsqueda por descripción.
//
// PROPÓSITO:
// - Abstracción de acceso a datos legacy
// - Consulta completa de modelos de documentos (catálogo pequeño ~38 registros)
// - Búsqueda eficiente por descripción
//
// =====================================================================================

using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para repositorio de modelos de documentos legacy (admDocumentosModelo de adCABS2016)
    /// Proporciona acceso a datos de modelos de documentos del sistema Adminpaq
    /// </summary>
    public interface IAdmDocumentoModeloRepository
    {
        /// <summary>
        /// Obtiene todos los modelos de documentos sin paginación
        /// </summary>
        /// <returns>Lista completa de modelos de documentos</returns>
        Task<List<AdmDocumentoModelo>> GetAllAsync();

        /// <summary>
        /// Busca modelos de documentos por descripción
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (descripción)</param>
        /// <returns>Lista de modelos de documentos filtrados</returns>
        Task<List<AdmDocumentoModelo>> SearchByDescripcionAsync(string searchTerm);
    }
}