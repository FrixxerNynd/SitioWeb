// =====================================================================================
// INTERFAZ SERVICE ADM ALMACEN - IAdmAlmacenService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de servicios de negocio para almacenes legacy.
// Sin cache Redis ya que es catálogo pequeño y estable.
//
// PROPÓSITO:
// - Lógica de negocio para almacenes legacy
// - Consulta directa sin cache (catálogo pequeño)
// - Mapeo de entidades a DTOs
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para servicio de almacenes legacy sin cache
    /// Proporciona operaciones de negocio para admAlmacenes de adCABS2016
    /// </summary>
    public interface IAdmAlmacenService
    {
        /// <summary>
        /// Obtiene todos los almacenes sin paginación ni cache
        /// </summary>
        /// <returns>Lista completa de almacenes como DTOs</returns>
        Task<List<AdmAlmacenResponseDto>> GetAllAsync();
    }
}