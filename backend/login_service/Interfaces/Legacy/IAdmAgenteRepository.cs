// =====================================================================================
// INTERFAZ REPOSITORY ADM AGENTE - IAdmAgenteRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para acceso a datos de agentes legacy (admAgentes).
// Especifica métodos paginados para consultas y búsquedas.
//
// PROPÓSITO:
// - Abstracción de acceso a datos legacy
// - Paginación para grandes volúmenes de agentes
// - Búsqueda eficiente por código y nombre
//
// =====================================================================================

using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para repositorio de agentes legacy (admAgentes de adCABS2016)
    /// Proporciona acceso paginado a datos de agentes del sistema Adminpaq
    /// </summary>
    public interface IAdmAgenteRepository
    {
        /// <summary>
        /// Obtiene un agente por su ID
        /// </summary>
        /// <param name="id">ID del agente</param>
        /// <returns>Agente con su domicilio</returns>
        Task<AdmAgente?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todos los agentes con paginación
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (1-100)</param>
        /// <returns>Tupla con lista de agentes y total de registros</returns>
        Task<(List<AdmAgente> Data, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Busca agentes por código o nombre con paginación
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (código o nombre)</param>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (1-100)</param>
        /// <returns>Tupla con lista de agentes filtrados y total de registros</returns>
        Task<(List<AdmAgente> Data, int TotalRecords)> SearchPaginatedAsync(string searchTerm, int page, int pageSize);

    }
}
