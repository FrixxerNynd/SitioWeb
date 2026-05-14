// =====================================================================================
// INTERFAZ SERVICE ADM CONCEPTO - IAdmConceptoService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de servicios de negocio para conceptos legacy.
// Sin cache Redis ya que es catálogo pequeño (~89 registros).
//
// PROPÓSITO:
// - Lógica de negocio para conceptos legacy
// - Consulta directa sin cache (catálogo pequeño)
// - Paginación y búsqueda optimizadas
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para servicio de conceptos legacy sin cache
    /// Proporciona operaciones de negocio para admConceptos de adCABS2016
    /// </summary>
    public interface IAdmConceptoService
    {
        /// <summary>
        /// Obtiene todos los conceptos con paginación sin cache
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (default: 50)</param>
        /// <returns>Respuesta paginada con conceptos y metadatos</returns>
        Task<PaginatedResponseDto<AdmConceptoResponseDto>> GetAllPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Busca conceptos por código o nombre con paginación sin cache
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (código o nombre)</param>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (default: 50)</param>
        /// <returns>Respuesta paginada con conceptos filtrados y metadatos</returns>
        Task<PaginatedResponseDto<AdmConceptoResponseDto>> SearchPaginatedAsync(string searchTerm, int page, int pageSize);
    }
}