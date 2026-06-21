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
        /// Verificar existencia de un cliente por ID
        /// </summary>
        Task<bool> ExistsAsync(int idCliente);

        /// <summary>
        /// Verificar estatus activo del cliente
        /// </summary>
        Task<bool> IsActiveAsync(int? idCliente);

        /// <summary>
        /// Búsqueda paginada de clientes con domicilio
        /// </summary>
        Task<(List<AdmClienteConDomicilioResponseDto> Clientes, int TotalRegistros, int TotalPaginas)> SearchPaginatedAsync(AdmClienteFilterDto filter);

        /// <summary>
        /// Obtener cliente por ID con domicilio
        /// </summary>
        Task<AdmClienteConDomicilioResponseDto?> GetByIdAsync(int idCliente, bool incluirDetalleUbicacion = true);
        
        /// <summary>
        /// Obtener el credito de un cliente por id
        /// </summary>
        /// <param name="idCliente"></param>
        /// <returns></returns>
        Task<CreditClientDto?> GetCreditByIdAsync(int idCliente);
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

        /// <summary>
        /// Obtiene lista paginada de clientes inactivos (CEstatus = 0), ordenados por fecha de alta más reciente
        /// </summary>
        Task<(List<AdmClienteConDomicilioResponseDto> Clientes, int TotalRegistros, int TotalPaginas)> GetClientesInactivosAsync(int numeroPagina = 1, int tamanoPagina = 50);

        /// <summary>
        /// Obtiene el detalle completo (datos + domicilio) de un cliente inactivo para su revisión previa a la activación
        /// </summary>
        Task<AdmClienteConDomicilioResponseDto?> GetDetalleClienteInactivoAsync(int idCliente);
    }
}
