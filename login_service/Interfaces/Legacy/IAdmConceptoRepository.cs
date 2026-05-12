// =====================================================================================
// INTERFAZ REPOSITORY ADM CONCEPTO - IAdmConceptoRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para acceso a datos de conceptos legacy (admConceptos).
// Especifica métodos paginados para consultas y búsquedas por código/nombre.
//
// PROPÓSITO:
// - Abstracción de acceso a datos legacy
// - Paginación para conceptos (89 registros en BD)
// - Búsqueda eficiente por código y nombre de concepto
//
// =====================================================================================

using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para repositorio de conceptos legacy (admConceptos de adCABS2016)
    /// Proporciona acceso paginado a datos de conceptos del sistema Adminpaq
    /// </summary>
    public interface IAdmConceptoRepository
    {
        /// <summary>
        /// Obtiene todos los conceptos con paginación
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (default: 50)</param>
        /// <returns>Tupla con lista de conceptos y total de registros</returns>
        Task<(List<AdmConcepto> Data, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Busca conceptos por código o nombre con paginación
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (código o nombre)</param>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (default: 50)</param>
        /// <returns>Tupla con lista de conceptos filtrados y total de registros</returns>
        Task<(List<AdmConcepto> Data, int TotalRecords)> SearchPaginatedAsync(string searchTerm, int page, int pageSize);
    }
}