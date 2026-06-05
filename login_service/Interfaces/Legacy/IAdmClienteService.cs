using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.DTOs.ServiceResponse;
using back_cabs.CRM.models.legacy;
using CRM.DTOs.Request;

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

        ///<summary>
        /// Registrar un cliente nuevo
        /// </summary>
        Task<ServiceResult<AdmClienteConDomicilioResponseDto>> RegistrarAsync(UserClientRequestDto clientData);

        /// <summary>
        /// Validar correo y contraseñas de un cliente para login (soporte legacy)
        /// </summary>
        Task<AdmCliente?> ValidateCredentialsAsync(string email, string contrasena);

        /// <summary>
        /// Autorizar un pre-registro de cliente nuevo
        /// </summary>
        Task<ServiceResult<AdmClienteConDomicilioResponseDto>> AutorizarClienteAsync(int idCliente, AutorizarClienteRequestDto autorizacionData);

        /// <summary>
        /// Busca un cliente por RFC o email (para flujo de recuperación de contraseña)
        /// </summary>
        Task<AdmCliente?> BuscarPorRfcOEmailAsync(string? rfc, string? email);

        /// <summary>
        /// Actualiza la contraseña (cifrada) de un cliente legacy por email
        /// </summary>
        Task<bool> ActualizarContrasenaAsync(string email, string nuevaPassword);
    }
}
