using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.models.Auth;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Auth
{
    /// <summary>
    /// Contrato para operaciones de acceso a datos de autenticación de usuarios.
    /// Define qué operaciones están disponibles sin especificar cómo se implementan.
    /// </summary>
    public interface IUsuarioAuthRepository
    {
        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// </summary>
        /// <param name="usuario">Entidad de usuario a crear</param>
        /// <returns>Usuario creado con ID asignado</returns>
        Task<UsuarioAuth> CreateAsync(UsuarioAuth usuario);

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<UsuarioAuth?> GetByEmailAsync(string email);

        /// <summary>
        /// Obtiene un usuario por su ID único
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<UsuarioAuth?> GetByIdAsync(int id);

        /// <summary>
        /// Verifica si existe un usuario con el email especificado
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <returns>True si existe, false si no</returns>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="usuario">Usuario con los datos actualizados</param>
        /// <returns>Usuario actualizado</returns>
        Task<UsuarioAuth> UpdateAsync(UsuarioAuth usuario);

        /// <summary>
        /// Valida las credenciales de un usuario
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Usuario si las credenciales son válidas, null si no</returns>
        Task<UsuarioAuth?> ValidateCredentialsAsync(string email, string password);

        /// <summary>
        /// Valida las credenciales de un usuario cliente
        /// </summary>
        /// <param name="id">ID del usuario cliente</param>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Usuario cliente si las credenciales son válidas, null si no</returns>
        Task<ServiceResult<Auth_cliente>> ValidateClientCredentialsAsync(int id, string password);

        /// <summary>
        /// Obtiene todos los usuarios del sistema
        /// </summary>
        /// <param name="incluirInactivos">Si incluye usuarios inactivos</param>
        /// <returns>Lista de usuarios</returns>
        Task<IEnumerable<UsuarioAuth>> GetAllAsync(bool incluirInactivos = false, int idCliente = 0);

        /// <summary>
        /// Obtiene usuarios filtrados por rol
        /// </summary>
        /// <param name="rol">Rol a filtrar (SOPORTE, ADMINISTRACION, RECEPCION)</param>
        /// <param name="incluirInactivos">Si incluye usuarios inactivos</param>
        /// <returns>Lista de usuarios del rol especificado</returns>
        Task<IEnumerable<UsuarioAuth>> GetByRolAsync(string rol, bool incluirInactivos = false);
            /// <summary>
        /// Obtiene múltiples usuarios por sus IDs
        /// </summary>
        /// <param name="ids">Lista de IDs</param>
        /// <returns>Lista de usuarios encontrados</returns>
        Task<IEnumerable<UsuarioAuth>> GetByIdsAsync(IEnumerable<int> ids);

        /// <summary>
        /// Inserta la contraseña de un usuario
        /// </summary>
        /// <param name="datos">Datos del usuario</param>
        /// <returns>Usuario actualizado</returns>
        Task<Auth_cliente> InsertPassword (Auth_cliente datos);

        /// <summary>
        /// Actualiza la contraseña de un usuario cliente
        /// </summary>
        /// <param name="id">ID del usuario cliente</param>
        /// <param name="newPassword">Contraseña en texto plano</param>
        /// <returns>Usuario cliente actualizado</returns>
        Task<Auth_cliente?> UpdateCredentialsAsync(int id, string newPassword);
    }
}