// =====================================================================================
// INTERFAZ SERVICE ADM AGENTE - IAdmAgenteService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de servicios de negocio para agentes legacy.
// Incluye cache Redis y paginación.
//
// PROPÓSITO:
// - Lógica de negocio para agentes legacy
// - Cache Redis con Cache-Aside pattern
// - Paginación y búsqueda optimizadas
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para servicio de agentes legacy con cache Redis
    /// Proporciona operaciones de negocio para admAgentes de adCABS2016
    /// </summary>
    public interface IAdmAgenteService
    {
        /// <summary>
        /// Obtiene todos los agentes con paginación y cache
        /// Cache TTL: 30 minutos por página
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (1-100, default: 30)</param>
        /// <returns>Respuesta paginada con agentes y metadatos</returns>
        Task<PaginatedResponseDto<AdmAgenteResponseDto>> GetAllPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Busca agentes por código o nombre con paginación y cache
        /// Cache TTL: 60 minutos por término de búsqueda y página
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (código o nombre)</param>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (1-100, default: 30)</param>
        /// <returns>Respuesta paginada con agentes filtrados y metadatos</returns>
        Task<PaginatedResponseDto<AdmAgenteResponseDto>> SearchPaginatedAsync(string searchTerm, int page, int pageSize);
    }
}
