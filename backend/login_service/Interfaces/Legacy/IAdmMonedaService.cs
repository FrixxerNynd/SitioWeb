// =====================================================================================
// INTERFAZ SERVICE ADM MONEDA - IAdmMonedaService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de servicios de negocio para monedas legacy.
// Incluye cache Redis y paginación.
//
// PROPÓSITO:
// - Lógica de negocio para monedas legacy
// - Cache Redis con Cache-Aside pattern
// - Paginación y consulta optimizadas
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para servicio de monedas legacy con cache Redis
    /// Proporciona operaciones de negocio para admMonedas de adCABS2016
    /// </summary>
    public interface IAdmMonedaService
    {
        /// <summary>
        /// Obtiene todas las monedas con paginación y cache
        /// Cache TTL: 60 minutos por página (catálogo pequeño y estable)
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Registros por página (1-100, default: 30)</param>
        /// <returns>Respuesta paginada con monedas y metadatos</returns>
        Task<PaginatedResponseDto<AdmMonedaResponseDto>> GetAllPaginatedAsync(int page, int pageSize);
    }
}