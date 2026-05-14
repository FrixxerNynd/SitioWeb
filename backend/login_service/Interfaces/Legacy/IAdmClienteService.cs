using back_cabs.CRM.DTOs.Legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interface para servicio de clientes con domicilio
    /// </summary>
    public interface IAdmClienteService
    {
        /// <summary>
        /// Búsqueda paginada de clientes con domicilio
        /// </summary>
        Task<(List<AdmClienteConDomicilioResponseDto> Clientes, int TotalRegistros, int TotalPaginas)> SearchPaginatedAsync(AdmClienteFilterDto filter);

        /// <summary>
        /// Obtener cliente por ID con domicilio
        /// </summary>
        Task<AdmClienteConDomicilioResponseDto?> GetByIdAsync(int idCliente, bool incluirDetalleUbicacion = true);
    }
}
