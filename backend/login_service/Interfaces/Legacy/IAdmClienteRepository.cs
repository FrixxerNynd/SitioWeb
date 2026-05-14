using back_cabs.CRM.DTOs.Legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interface para repositorio de clientes con domicilio
    /// </summary>
    public interface IAdmClienteRepository
    {
        /// <summary>
        /// Búsqueda paginada de clientes con filtros
        /// </summary>
        Task<(List<models.legacy.AdmCliente> Clientes, int TotalRegistros)> SearchPaginatedAsync(AdmClienteFilterDto filter);

        /// <summary>
        /// Obtener cliente por ID con su domicilio
        /// </summary>
        Task<models.legacy.AdmCliente?> GetByIdWithDomicilioAsync(int idCliente, int? tipoDireccion = 1);
    }
}
