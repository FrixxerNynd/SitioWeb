// =====================================================================================
// INTERFAZ REPOSITORY ADM ALMACEN - IAdmAlmacenRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para acceso a datos de almacenes legacy (admAlmacenes).
// Especifica método simple para obtener todos los almacenes (catálogo pequeño).
//
// PROPÓSITO:
// - Abstracción de acceso a datos legacy
// - Consulta completa de almacenes (solo ~5 registros)
// - Optimización para catálogo pequeño sin paginación
//
// =====================================================================================

using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para repositorio de almacenes legacy (admAlmacenes de adCABS2016)
    /// Proporciona acceso completo a datos de almacenes del sistema Adminpaq
    /// </summary>
    public interface IAdmAlmacenRepository
    {
        /// <summary>
        /// Obtiene todos los almacenes sin paginación
        /// </summary>
        /// <returns>Lista completa de almacenes</returns>
        Task<List<AdmAlmacen>> GetAllAsync();
    }
}