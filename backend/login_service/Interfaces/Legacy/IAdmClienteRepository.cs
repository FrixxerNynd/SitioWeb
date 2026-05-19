using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interface para repositorio de clientes con domicilio
    /// </summary>
    public interface IAdmClienteRepository
    {
        /// <summary>
        /// Inserta un nuevo domicilio para un cliente existente
        /// </summary>
        Task<AdmDomicilio> InsertDomicilioAsync(AdmDomicilio domicilio);

        /// <summary>
        /// Inserta un nuevo cliente en la base de datos
        /// </summary>
        Task<AdmCliente> InsertAsync(AdmCliente cliente);

        /// <summary>
        /// Búsqueda paginada de clientes con filtros
        /// </summary>
        Task<(List<models.legacy.AdmCliente> Clientes, int TotalRegistros)> SearchPaginatedAsync(AdmClienteFilterDto filter);

        /// <summary>
        /// Obtener cliente por ID con su domicilio
        /// </summary>
        Task<models.legacy.AdmCliente?> GetByIdWithDomicilioAsync(int idCliente, int? tipoDireccion = 1);
    
        /// <summary>
        /// Validar credenciales (soporte legacy)
        /// </summary>
        Task<models.legacy.AdmCliente?> ValidateCredentialsAsync(string email, string contrasena);
    }
}
