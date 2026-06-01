// =====================================================================================
// INTERFAZ REPOSITORY ADM MONEDA - IAdmMonedaRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para acceso a datos de monedas legacy (admMonedas).
// Especifica métodos paginados para consultas (sin búsqueda ya que es catálogo pequeño).
//
// PROPÓSITO:
// - Abstracción de acceso a datos legacy
// - Paginación para catálogo de monedas
// - Consulta optimizada de monedas del sistema Adminpaq
//
// =====================================================================================

using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para repositorio de monedas legacy (admMonedas de adCABS2016)
    /// Proporciona acceso paginado a datos de monedas del sistema Adminpaq
    /// </summary>
    public interface IAdmMonedaRepository
    {
        /// <summary>
        /// Obtiene todas las monedas con paginación
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (1-100)</param>
        /// <returns>Tupla con lista de monedas y total de registros</returns>
        Task<(List<AdmMoneda> Data, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize);
    }
}