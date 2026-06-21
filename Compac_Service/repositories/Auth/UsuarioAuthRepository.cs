using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Auth;
using back_cabs.CRM.models.Auth;
using back_cabs.CRM.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using back_cabs.CRM.DTOs;
using back_cabs.CRM.DTOs.ServiceResponse;

namespace back_cabs.CRM.repositories.Auth
{
    /// <summary>
    /// Implementación de IUsuarioAuthRepository usando Entity Framework Core.
    /// Encapsula todas las operaciones de BD para autenticación de usuarios.
    /// </summary>
    public class UsuarioAuthRepository : IUsuarioAuthRepository
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly LegacyCompacReadOnlyContext _legacyReadContext;
        private readonly ILogger<UsuarioAuthRepository> _logger;

        public UsuarioAuthRepository(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            LegacyCompacReadOnlyContext legacyReadContext,
            ILogger<UsuarioAuthRepository> logger)
        {
            _writeContext = writeContext;
            _readContext = readContext;
            _legacyReadContext = legacyReadContext;
            _logger = logger;
        }


        // ✏️ IMPLEMENTACIÓN DE ESCRITURAS

        public async Task<UsuarioAuth> CreateAsync(UsuarioAuth usuario)
        {
            try
            {
                _writeContext.UsuariosAuth.Add(usuario);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Usuario creado con ID {Id} y email {Email}",
                    usuario.Id, usuario.Email);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario con email {Email}", usuario.Email);
                throw;
            }
        }

        public async Task<UsuarioAuth> UpdateAsync(UsuarioAuth usuario)
        {
            try
            {
                _writeContext.UsuariosAuth.Update(usuario);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Usuario actualizado con ID {Id}", usuario.Id);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID {Id}", usuario.Id);
                throw;
            }
        }

        public async Task<Auth_cliente> InsertPassword (Auth_cliente datos)
        {
            try
            {
                _writeContext.Auth_Clientes.Add(datos);
                await _writeContext.SaveChangesAsync();
                _logger.LogInformation("Contraseña insertada con exito para usuario con ID {Id}", datos.Id_Cliente);
                return datos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al insertar contraseña para usuario con ID {Id}", datos.Id_Cliente);
                throw;
            }
        }

        // 📖 IMPLEMENTACIÓN DE LECTURAS

        public async Task<UsuarioAuth?> GetByEmailAsync(string email)
        {
            try
            {
                var usuario = await _readContext.UsuariosAuth
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (usuario == null)
                {
                    _logger.LogDebug("Usuario con email {Email} no encontrado", email);
                }

                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por email {Email}", email);
                throw;
            }
        }

        public async Task<UsuarioAuth?> GetByIdAsync(int id)
        {
            try
            {
                var usuario = await _readContext.UsuariosAuth
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    _logger.LogDebug("Usuario con ID {Id} no encontrado", id);
                }

                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                var exists = await _readContext.UsuariosAuth
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());

                _logger.LogDebug("Email {Email} existe: {Exists}", email, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de email {Email}", email);
                throw;
            }
        }

        public async Task<UsuarioAuth?> ValidateCredentialsAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Validando credenciales para usuario: {Email}", email);

                // Buscar usuario por email
                var usuario = await _readContext.UsuariosAuth
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Email}", email);
                    return null;
                }

                // Validar contraseña (soportar tanto Password en texto plano como Hash)
                bool contrasenaValida = false;

                // Primero intentar con password en texto plano
                if (!string.IsNullOrEmpty(usuario.Password) && usuario.Password == password)
                {
                    contrasenaValida = true;
                }
                // Si no, intentar con hash
                else if (!string.IsNullOrEmpty(usuario.Password))
                {
                    var contrasenaHash = ApiUtilities.GenerateSha256Hash(password);
                    contrasenaValida = usuario.Password == contrasenaHash;
                }

                if (!contrasenaValida)
                {
                    _logger.LogWarning("Contraseña inválida para usuario: {Email}", email);
                    return null;
                }

                _logger.LogInformation("Credenciales válidas para usuario: {Email}", email);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando credenciales para usuario: {Email}", email);
                throw;
            }
        }

        public async Task<ServiceResult<Auth_cliente>> ValidateClientCredentialsAsync(int id, string password)
        {
            _logger.LogInformation("Validando credenciales para cliente con ID {Id}", id);
            try
            {
                var datosCliente = await _readContext.Auth_Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id_Cliente == id);

                var Cliente = await _legacyReadContext.AdmClientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CIdClienteProveedor == id);

                // ✅ Primero verificar nulls ANTES de acceder a propiedades
                if (datosCliente == null || Cliente == null)
                {
                    _logger.LogWarning("Cliente con ID {Id} no encontrado", id);
                    return ServiceResult<Auth_cliente>.Fail("Cliente no encontrado");
                }

                // ✅ Ahora es seguro acceder a CEstatus
                if (Cliente.CEstatus == 0)
                {
                    _logger.LogWarning("Cliente con ID {Id} inactivo", id);
                    return ServiceResult<Auth_cliente>.Fail("Cliente inactivo");
                }

                if (string.IsNullOrEmpty(datosCliente.password))
                {
                    _logger.LogWarning("Cliente con ID {Id} no tiene contraseña configurada", id);
                    return ServiceResult<Auth_cliente>.Fail("Cliente no tiene contraseña configurada");
                }

                // Comparar siempre contra hash
                var contrasenaHash = ApiUtilities.GenerateSha256Hash(password);
                bool contrasenaValida = datosCliente.password == contrasenaHash;

                if (!contrasenaValida)
                {
                    _logger.LogWarning("Contraseña inválida para cliente con ID {Id}", id);
                    return ServiceResult<Auth_cliente>.Fail("Contraseña inválida");
                }

                _logger.LogInformation("Credenciales válidas para cliente con ID {Id}", id);
                return ServiceResult<Auth_cliente>.Ok(datosCliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando credenciales para cliente con ID {Id}", id);
                return ServiceResult<Auth_cliente>.Fail("Error al validar credenciales");
            }
        }


        public async Task<IEnumerable<UsuarioAuth>> GetAllAsync(bool incluirInactivos = false, int idCliente = 0)
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los usuarios (incluir inactivos: {IncluirInactivos})", incluirInactivos);

                var query = _readContext.UsuariosAuth.AsNoTracking();

                if (!incluirInactivos)
                {
                    query = query.Where(u => u.Activo);
                }

                var usuarios = await query
                    .OrderBy(u => u.Nombre)
                    .ThenBy(u => u.Apellido)
                    .Where(u => u.IdAgenteLegacy == idCliente)
                    .ToListAsync();

                _logger.LogInformation("Se obtuvieron {Count} usuarios", usuarios.Count);
                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                throw;
            }
        }

        public async Task<IEnumerable<UsuarioAuth>> GetByRolAsync(string rol, bool incluirInactivos = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuarios por rol: {Rol} (incluir inactivos: {IncluirInactivos})", 
                    rol, incluirInactivos);

                var query = _readContext.UsuariosAuth
                    .AsNoTracking()
                    .Where(u => u.Rol.ToUpper() == rol.ToUpper());

                if (!incluirInactivos)
                {
                    query = query.Where(u => u.Activo);
                }

                var usuarios = await query
                    .OrderBy(u => u.Nombre)
                    .ThenBy(u => u.Apellido)
                    .ToListAsync();

                _logger.LogInformation("Se obtuvieron {Count} usuarios con rol {Rol}", usuarios.Count, rol);
                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por rol: {Rol}", rol);
                throw;
            }
        }
        public async Task<IEnumerable<UsuarioAuth>> GetByIdsAsync(IEnumerable<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return new List<UsuarioAuth>();
                }

                var uniqueIds = ids.Distinct().ToList();
                _logger.LogInformation("Obteniendo {Count} usuarios por lista de IDs", uniqueIds.Count);

                var usuarios = await _readContext.UsuariosAuth
                    .AsNoTracking()
                    .Where(u => uniqueIds.Contains(u.Id))
                    .ToListAsync();

                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por lista de IDs");
                throw;
            }
        }

        public async Task<Auth_cliente?> UpdateCredentialsAsync(int id, string newPassword)
        {
            try
            {
                var datosCliente = await _readContext.Auth_Clientes
                    .FirstOrDefaultAsync(c => c.Id_Cliente == id);
                
                if(datosCliente == null)
                {
                    _logger.LogWarning("Cliente con ID {Id} no encontrado", id);
                    return null;
                }
                datosCliente.password = newPassword;
                _writeContext.Auth_Clientes.Update(datosCliente);
                await _writeContext.SaveChangesAsync();
                _logger.LogInformation("Credenciales actualizadas para cliente con ID {Id}", id);
                return datosCliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar credenciales del cliente con ID {Id}", id);
                throw;
            }
        }
    }
}