//file:back_cabs/CRM/controllers/Auth/AuthController.cs
// =====================================================================================
// CONTROLADOR AUTH - AuthController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los endpoints HTTP para operaciones de autenticación y gestión de usuarios.
// Incluye registro, login, y otras operaciones relacionadas con la autenticación.
//
// CUÁNDO USARLO:
// - Registro de nuevos usuarios (POST /api/auth/registro)
// - Login de usuarios existentes
// - Operaciones de gestión de autenticación
// - Endpoints públicos de autenticación
//
// CÓMO USARLO:
// Los endpoints se exponen automáticamente en:
// POST /api/auth/registro - Registrar nuevo usuario
// Documentación completa disponible en Swagger UI
//
// =====================================================================================

using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.services.Auth;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.Auth;
using back_cabs.CRM.models;
using back_cabs.CRM.Middleware;
using back_cabs.CRM.utils.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net;
using login_service.services;

namespace back_cabs.CRM.controllers.Auth
{
    /// <summary>
    /// Controlador para operaciones de autenticación y gestión de usuarios
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    
    {
        private readonly UsuarioAuthService _usuarioAuthService;
        private readonly IAdmClienteService _admClienteService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAntiforgery _antiforgery;
        private readonly EmailService _emailService;
        private readonly RecaptchaService _recaptchaService;
        private readonly HmacOtpService _otpService;


        public AuthController(
            UsuarioAuthService usuarioAuthService,
            ILogger<AuthController> logger,
            IConfiguration configuration,
            IAntiforgery antiforgery,
            EmailService emailService,
            IAdmClienteService admClienteService,
            RecaptchaService recaptchaService,
            HmacOtpService otpService)
        {
            _usuarioAuthService = usuarioAuthService ?? throw new ArgumentNullException(nameof(usuarioAuthService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _admClienteService = admClienteService ?? throw new ArgumentNullException(nameof(admClienteService));
            _recaptchaService = recaptchaService;
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
        }
        ///<summary>
        /// Registrar un nuevo cliente en el sistema
        /// </summary>
        /// <param name="request"> Datos del cliente</param>
        /// <returns> Mensaje con solicitud enviada</returns>
        [HttpPost("registro-cliente")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RegistrarCliente([FromBody] UserClientRequestDto request)
        {
            try
            {
                //Validar reCAPTCHA v2
                // var recaptchaValido = await _recaptchaService.ValidarV2Async(request.RecaptchaToken);
                // if (!recaptchaValido)
                // {
                //     _logger.LogWarning("Registro bloqueado por reCAPTCHA v2 para email: {Email}", request.Email);
                //     return BadRequest(new { message = "Por favor completa el reCAPTCHA." });
                // }
                
                _logger.LogInformation("Iniciando registro de cliente para email: {Email}", request?.Email);
                // Validación básica del request
                if (request == null)
                {
                    _logger.LogWarning("Request de registro recibido como null");
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Los datos del cliente son requeridos",
                        "Request body cannot be null"));
                }

                //Procesar el registro
                var resultado = await _admClienteService.RegistrarAsync(request);
                if (!resultado.Success)
                {
                    _logger.LogWarning("Error al registrar cliente: {Mensaje}", resultado.Message);
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Error al registrar cliente",
                        resultado.Message));
                }
                _logger.LogInformation("Cliente registrado exitosamente: {Nombre} - {Email}",
                    request.Nombre, request.Email);
                // Enviar email de confirmación (opcional)
                // await _emailService.EnviarEmailAsync(request.Email, "Registro exitoso", $"Hola {request.Nombre}, tu registro como cliente ha sido exitoso.");

                // Retornar respuesta exitosa con código 201 Created
                return StatusCode(201, new
                {
                    success = true,
                    message = "Cliente registrado exitosamente, en espera de aceptacion"
                });
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning("Errores de validación en registro de cliente: {Errores}",
                    string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                // Crear respuesta estructurada con errores de validación
                var erroresValidacion = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Errores de validación en los datos proporcionados",
                    System.Text.Json.JsonSerializer.Serialize(erroresValidacion)));
            }
        }


        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="request">Datos del usuario a registrar</param>
        /// <returns>Información del usuario registrado con token JWT</returns>
        /// <response code="201">Usuario registrado exitosamente</response>
        /// <response code="400">Datos de entrada inválidos o email duplicado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("registro")]
        [ProducesResponseType(typeof(RegistroExitosoResponseDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioRegistroRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando registro de usuario para email: {Email}", request?.Email);

                // Validación básica del request
                if (request == null)
                {
                    _logger.LogWarning("Request de registro recibido como null");
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Los datos del usuario son requeridos",
                        "Request body cannot be null"));
                }

                // Procesar el registro
                var resultado = await _usuarioAuthService.RegistrarUsuarioAsync(request);

                _logger.LogInformation("Usuario registrado exitosamente: {UserId} - {Email}",
                    resultado.Usuario.Id, resultado.Usuario.Email);

                // Retornar respuesta exitosa con código 201 Created
                return StatusCode(201, resultado);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning("Errores de validación en registro: {Errores}",
                    string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

                // Crear respuesta estructurada con errores de validación
                var erroresValidacion = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Errores de validación en los datos proporcionados",
                    System.Text.Json.JsonSerializer.Serialize(erroresValidacion)));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("email"))
            {
                _logger.LogWarning("Intento de registro con email duplicado: {Email}", request?.Email);

                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Email duplicado",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante registro de usuario: {Email}", request?.Email);

                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error interno del servidor",
                    "Ocurrió un error inesperado durante el proceso de registro"));
            }
        }

        /// <summary>
        /// Autorizar cliente nuevo en preregistro
        /// </summary>
        /// <response code="200">Cliente autorizado exitosamente</response>
        /// <response code="404">Cliente no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("autorizar-cliente")]
        [Authorize]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AutorizarCliente([FromBody] AutorizarClienteRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando autorización de cliente para email: {Id}", request?.Id);

                // Validación básica del request
                if (request == null)
                {
                    _logger.LogWarning("Request de autorización recibido como null");
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Los datos del cliente son requeridos",
                        "Request body cannot be null"));
                }

                // Procesar la autorización
                var resultado = await _admClienteService.AutorizarClienteAsync(request.Id, request);

                _logger.LogInformation("Cliente autorizado exitosamente: {Id}", request?.Id);

                // Retornar respuesta exitosa con código 200 OK
                return Ok(resultado);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning("Errores de validación en autorización: {Errores}",
                    string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

                // Crear respuesta estructurada con errores de validación
                var erroresValidacion = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Errores de validación en los datos proporcionados",
                    System.Text.Json.JsonSerializer.Serialize(erroresValidacion)));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("no encontrado"))
            {
                _logger.LogWarning("Cliente no encontrado para autorización: {Id}", request?.Id);

                return NotFound(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorNoEncontrado,
                    "Cliente no encontrado",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante autorización de cliente: {Id}", request?.Id);

                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error interno del servidor",
                    "Ocurrió un error inesperado durante el proceso de autorización"));
            }
        }


        /// <summary>
        /// ✅ MEJORA 5: Obtiene un token CSRF para proteger requests subsecuentes
        /// </summary>
        /// <remarks>
        /// Este endpoint debe ser llamado antes de cualquier operación POST/PUT/DELETE.
        /// El token se almacena en una cookie y también se retorna en el body.
        /// 
        /// Flujo:
        /// 1. Cliente hace GET /api/auth/csrf-token
        /// 2. Server genera token y lo almacena en cookie XSRF-TOKEN
        /// 3. Cliente incluye token en header X-XSRF-TOKEN en requests POST/PUT/DELETE
        /// </remarks>
        /// <returns>Token CSRF y su nombre de header</returns>
        /// <response code="200">Token CSRF generado exitosamente</response>
        [HttpGet("csrf-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public IActionResult GetCsrfToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

            _logger.LogDebug("CSRF token generated for {IpAddress}",
                HttpContext.Connection.RemoteIpAddress);

            return Ok(new
            {
                csrfToken = tokens.RequestToken,
                headerName = tokens.HeaderName ?? "X-XSRF-TOKEN",
                message = "Include this token in the X-XSRF-TOKEN header for POST/PUT/DELETE requests"
            });
        }

        /// <summary>
        /// Verifica si un email ya está registrado en el sistema
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <returns>Indica si el email existe o no</returns>
        /// <response code="200">Verificación completada</response>
        /// <response code="400">Email inválido</response>
        [HttpGet("verificar-email")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> VerificarEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Email requerido",
                        "El email es obligatorio para la verificación"));
                }

                var existe = await _usuarioAuthService.EmailExisteAsync(email);

                return Ok(new
                {
                    Email = email,
                    Existe = existe,
                    Disponible = !existe,
                    Mensaje = existe ? "Email ya registrado" : "Email disponible"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar email: {Email}", email);
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error en verificación",
                    "No se pudo verificar la disponibilidad del email"));
            }
        }

        /// <summary>
        /// Autentica un usuario en el sistema
        /// </summary>
        /// <param name="request">Credenciales de login</param>
        /// <returns>Token JWT y información del usuario autenticado</returns>
        /// <response code="200">Login exitoso</response>
        /// <response code="401">Credenciales inválidas</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                //Validar reCAPTCHA V3
                //  var recaptchaValido = await _recaptchaService.ValidarV3Async(request.RecaptchaToken);
                // if (!recaptchaValido)
                // {
                //      _logger.LogWarning("Login bloqueado por reCAPTCHA v3 para email: {Email}", request.Email);
                //     return BadRequest(new { message = "Validación de seguridad fallida. Intenta de nuevo." });
                // }

                // Validar credenciales con base de datos
                var usuario = await _usuarioAuthService.ValidarCredencialesAsync(request.Email, request.Password);
                if (usuario == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Determinar rol (por defecto Recepcion si no está definido)

                var user = new User
                {
                    Id = usuario.Id.ToString(),
                    Email = usuario.Email,
                    Name = usuario.NombreCompleto,
                    Role = usuario.Rol,
                    IdAgenteLegacy = usuario.IdAgenteLegacy
                };
                var tokens = GenerateTokens(user);

                // ✅ MEJORA 2: Usar CookieHelper con flags de seguridad completas
                // HttpOnly=true, Secure=true, SameSite=Strict
                CookieHelper.SetSecureJwtCookie(Response, tokens.AccessToken, expiryMinutes: 30);

                _logger.LogInformation($"Usuario {user.Email} inició sesión exitosamente");

                return Ok(new
                {
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        name = user.Name,
                        role = user.Role,
                        permissions = user.Permissions,
                        idAgente = user.IdAgenteLegacy
                    },
                    // El token se envía en la cookie, pero lo devolvemos para facilitar pruebas en Swagger
                    token = tokens.AccessToken,
                    refreshToken = tokens.RefreshToken, // ✅ NUEVO: Token de refresco en la respuesta
                    expiresIn = DateTime.UtcNow.AddMinutes(540)  // 30 minutos en segundos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Refresca el token de acceso usando el refresh token
        /// </summary>
        /// <returns>Nuevo token de acceso y refresh token</returns>
        /// <response code="200">Tokens refrescados exitosamente</response>
        /// <response code="401">Refresh token inválido o expirado</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto? request)
        {
            try
            {
                // 1. Intentar obtener el refresh token de la cookie
                var refreshToken = Request.Cookies["RefreshToken"];

                // 2. Si no hay cookie, intentar obtener del body (para clientes no-browser)
                if (string.IsNullOrEmpty(refreshToken) && request != null)
                {
                    refreshToken = request.RefreshToken;
                }

                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Refresh Token fallido: Token no proporcionado ni en cookie ni en body");
                    return Unauthorized(new { message = "Token de refresco no proporcionado" });
                }

                if (!IsValidRefreshToken(refreshToken))
                {
                    _logger.LogWarning("Refresh Token fallido: Token con formato inválido: {Token}", refreshToken);
                    return Unauthorized(new { message = "Token de refresco inválido" });
                }

                // 3. Extraer ID de usuario
                // TODO: En producción validar firma/BD real del refresh token
                var userIdStr = GetUserIdFromRefreshToken(refreshToken).Trim(); // ✅ Add Trim()

                // Add explicit debug log
                _logger.LogInformation("Refresh Token parsing: Token '{Token}' -> Extracted ID String '{IdStr}'", refreshToken, userIdStr);

                if (!int.TryParse(userIdStr, out var userId))
                {
                    _logger.LogWarning("Refresh Token fallido: ID de usuario no es un número válido: '{IdString}'", userIdStr);
                    return Unauthorized(new { message = "Formato de token erróneo" });
                }

                _logger.LogInformation("Refresh Token: Buscando usuario con ID int: {UserId}", userId);

                // 4. Obtener usuario REAL de la BD
                var usuario = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(userId);

                if (usuario == null)
                {
                    _logger.LogWarning("Refresh Token fallido: Usuario NULL devuelto por servicio para ID: {UserId}", userId);
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                if (!usuario.Activo)
                {
                    _logger.LogWarning("Refresh Token fallido: Usuario inactivo: {Email}", usuario.Email);
                    return Unauthorized(new { message = "Usuario inactivo" });
                }

                // 5. Generar nuevos tokens
                
                var userModel = new User
                {
                    Id = usuario.Id.ToString(),
                    Email = usuario.Email,
                    Name = usuario.NombreCompleto,
                };

                var tokens = GenerateTokens(userModel);

                // 6. Actualizar cookies HttpOnly (siempre útil para navegadores)
                SetRefreshTokenCookie(tokens.RefreshToken);
                SetAccessTokenCookie(tokens.AccessToken);

                _logger.LogInformation($"Tokens refrescados exitosamente para usuario {usuario.Email}");

                // 7. Retornar tokens en el body también
                return Ok(new
                {
                    accessToken = tokens.AccessToken,
                    refreshToken = tokens.RefreshToken,
                    expiresIn = 1800 // 30 minutos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante el refresh token");
                return Unauthorized(new { message = "Error al procesar la solicitud de refresh" });
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario
        /// </summary>
        /// <returns>Confirmación de logout</returns>
        /// <response code="200">Logout exitoso</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public Task<IActionResult> Logout()
        {
            try
            {
                // Limpiar cookies HttpOnly
                ClearAuthCookies();

                _logger.LogInformation("Usuario cerró sesión");

                return Task.FromResult<IActionResult>(Ok(new { message = "Sesión cerrada exitosamente" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el logout");
                return Task.FromResult<IActionResult>(StatusCode(500, new { message = "Error al cerrar sesión" }));
            }
        }

        /// <summary>
        /// Obtiene información completa del usuario actualmente autenticado
        /// </summary>
        /// <returns>Información completa del usuario actual</returns>
        /// <response code="200">Usuario obtenido exitosamente</response>
        /// <response code="401">Token inválido o no proporcionado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(UserMeResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Extraer el ID del usuario desde los claims del JWT
                var userIdClaim = User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("Usuario autenticado pero sin claim NameIdentifier. Claims disponibles: {Claims}",
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                    return Unauthorized(new { message = "Token de acceso inválido - falta ID de usuario" });
                }

                // Validar y parsear el ID del usuario
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("ID de usuario inválido en claims: {UserId}", userIdClaim);
                    return Unauthorized(new { message = "ID de usuario inválido en el token" });
                }

                // Obtener los datos completos del usuario desde la base de datos
                var usuario = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(userId);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado en BD para ID: {UserId}", userId);
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                _logger.LogInformation("Usuario {Email} (ID: {UserId}) obtuvo su información exitosamente",
                    usuario.Email, usuario.Id);


                // Mapear a DTO de respuesta completo
                var response = new UserMeResponseDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    NombreCompleto = usuario.NombreCompleto,
                    Telefono = usuario.Telefono,
                    Activo = usuario.Activo,
                    TransmisionHabilitada = usuario.TransmisionHabilitada,
                    PuedeUsarVehiculo = usuario.PuedeUsarVehiculo,
                    CreadoEn = usuario.CreadoEn,
                    ActualizadoEn = usuario.ActualizadoEn,
                    IdAgenteLegacy = usuario.IdAgenteLegacy
                };

                return Ok(response);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Error de formato al procesar claims de usuario");
                return Unauthorized(new { message = "Token inválido - formato incorrecto" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener usuario actual");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios del sistema
        /// </summary>
        /// <param name="incluirInactivos">Si incluye usuarios inactivos (opcional, default: false)</param>
        /// <returns>Lista de todos los usuarios</returns>
        /// <response code="200">Lista de usuarios obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("usuarios")]
        [Authorize(Roles = "ADMINISTRACION")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllUsuarios([FromQuery] bool incluirInactivos = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los usuarios (incluir inactivos: {IncluirInactivos})", incluirInactivos);

                var usuarios = await _usuarioAuthService.ObtenerTodosLosUsuariosAsync(incluirInactivos);

                return Ok(new
                {
                    success = true,
                    data = usuarios,
                    count = usuarios.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Error al obtener los usuarios",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene usuarios filtrados por rol (para selectores de técnicos, etc.)
        /// </summary>
        /// <param name="rol">Rol a filtrar (SOPORTE, ADMINISTRACION, RECEPCION)</param>
        /// <param name="incluirInactivos">Si incluye usuarios inactivos (opcional, default: false)</param>
        /// <returns>Lista de usuarios del rol especificado</returns>
        /// <response code="200">Lista de usuarios obtenida exitosamente</response>
        /// <response code="400">Rol inválido</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("usuarios/rol/{rol}")]
        [Authorize(Roles = "ADMINISTRACION, RECEPCION, SOPORTE")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsuariosPorRol(string rol, [FromQuery] bool incluirInactivos = false)
        {
            try
            {
                // Validar que el rol sea válido
                var rolesValidos = new[] { "SOPORTE", "ADMINISTRACION", "RECEPCION" };
                if (!rolesValidos.Contains(rol.ToUpper()))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Rol inválido. Roles válidos: {string.Join(", ", rolesValidos)}"
                    });
                }

                _logger.LogInformation("Obteniendo usuarios por rol: {Rol} (incluir inactivos: {IncluirInactivos})",
                    rol, incluirInactivos);

                var usuarios = await _usuarioAuthService.ObtenerUsuariosPorRolAsync(rol, incluirInactivos);

                return Ok(new
                {
                    success = true,
                    data = usuarios,
                    count = usuarios.Count(),
                    rol = rol.ToUpper()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por rol: {Rol}", rol);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = $"Error al obtener usuarios con rol {rol}",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene técnicos disponibles (SOPORTE y ADMINISTRACION activos)
        /// Endpoint simplificado para selectores en formularios
        /// </summary>
        /// <returns>Lista de técnicos disponibles</returns>
        /// <response code="200">Lista de técnicos obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("tecnicos")]
        [Authorize(Roles = "ADMINISTRACION, RECEPCION, SOPORTE")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTecnicos()
        {
            try
            {
                _logger.LogInformation("Obteniendo técnicos disponibles (SOPORTE y ADMINISTRACION)");

                // Obtener usuarios de SOPORTE
                var soporte = await _usuarioAuthService.ObtenerUsuariosPorRolAsync("SOPORTE", incluirInactivos: false);

                // Obtener usuarios de ADMINISTRACION
                var administracion = await _usuarioAuthService.ObtenerUsuariosPorRolAsync("ADMINISTRACION", incluirInactivos: false);

                // Combinar ambas listas
                var tecnicos = soporte.Concat(administracion)
                    .OrderBy(u => u.Nombre)
                    .ThenBy(u => u.Apellido)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = tecnicos,
                    count = tecnicos.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener técnicos disponibles");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Error al obtener técnicos disponibles",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene usuarios con rol SOPORTE (para delegación de ejecuciones)
        /// </summary>
        /// <returns>Lista de usuarios con rol SOPORTE activos</returns>
        /// <response code="200">Lista obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("usuarios/soporte")]
        [AllowAnonymous] // Sin restricciones para evitar errores de permisos
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsuariosSoporte()
        {
            try
            {
                _logger.LogInformation("Obteniendo usuarios con rol SOPORTE");

                var usuarios = await _usuarioAuthService.ObtenerUsuariosPorRolAsync("SOPORTE", incluirInactivos: false);

                return Ok(new
                {
                    success = true,
                    data = usuarios.Select(u => new
                    {
                        id = u.Id,
                        nombre = u.Nombre,
                        apellido = u.Apellido,
                        nombreCompleto = $"{u.Nombre} {u.Apellido}",
                        email = u.Email,
                        activo = u.Activo
                    }).ToList(),
                    count = usuarios.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios SOPORTE");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Error al obtener usuarios SOPORTE",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado
        /// </summary>
        /// <param name="request">Contraseña actual y nueva contraseña</param>
        /// <returns>Confirmación del cambio de contraseña</returns>
        /// <response code="200">Contraseña cambiada exitosamente</response>
        /// <response code="400">Contraseña actual incorrecta</response>
        [HttpPost("recuperar-cuenta")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RecuperarCuenta([FromBody] SolicitudRecuperacionDto request)
        {
            try
            {
                // Validar que venga al menos un identificador
                if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Rfc))
                    return BadRequest(new { message = "Se requiere email o RFC para recuperar la cuenta." });

                string? emailDestino = null;

                // ── 1. Buscar en UsuariosAuth (usuarios internos) por email ──────────
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var usuarioInterno = await _usuarioAuthService.ObtenerUsuarioPorEmailAsync(request.Email);
                    if (usuarioInterno != null)
                    {
                        emailDestino = usuarioInterno.Email;
                        _logger.LogInformation("Recuperación solicitada para usuario interno: {Email}", emailDestino);
                    }
                }

                // ── 2. Si no se encontró internamente, buscar en AdmClientes ─────────
                if (emailDestino == null)
                {
                    var clienteLegacy = await _admClienteService.BuscarPorRfcOEmailAsync(
                        rfc: request.Rfc,
                        email: request.Email);

                    if (clienteLegacy != null)
                    {
                        emailDestino = !string.IsNullOrWhiteSpace(clienteLegacy.CEmail1) ? clienteLegacy.CEmail1
                                    : !string.IsNullOrWhiteSpace(clienteLegacy.CEmail2) ? clienteLegacy.CEmail2
                                    : clienteLegacy.CEmail3;

                        if (!string.IsNullOrWhiteSpace(emailDestino))
                            _logger.LogInformation("Recuperación solicitada para cliente legacy RFC: {Rfc}", request.Rfc);
                    }
                }

                // ── 3. Respuesta genérica si no se encontró (no revelar si existe) ──
                if (string.IsNullOrWhiteSpace(emailDestino))
                {
                    _logger.LogWarning("Recuperación para RFC/email no registrado: {Rfc}/{Email}",
                        request.Rfc, request.Email);
                    // Misma respuesta intencionalmente para no revelar si el email existe
                    return Ok(new { message = "Si existe una cuenta con ese dato, recibirás un correo de recuperación." });
                }

                // ── 4. Generar código OTP HMAC (sin BD) ────────────────────────────
                var codigo = _otpService.GenerarCodigo(emailDestino);

                // ── 5. Enviar correo ─────────────────────────────────────────────
                await _emailService.EnviarTokenRecuperacionAsync(emailDestino, codigo);

                return Ok(new { message = "Si existe una cuenta con ese dato, recibirás un correo de recuperación." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar solicitud de recuperación. RFC: {Rfc} / Email: {Email}",
                    request.Rfc, request.Email);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("cambiar-contraseña-recuperacion")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CambiarContrasenaRecuperacion([FromBody] CambioContrasenaRecuperacionDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.NuevoPassword) ||
                    string.IsNullOrWhiteSpace(request.Token))
                    return BadRequest(new { message = "Email, nueva contraseña y código son requeridos." });

                // ── 1. Validar código OTP HMAC (sin BD) ───────────────────────────
                if (!_otpService.ValidarCodigo(request.Email, request.Token))
                    return BadRequest(new { message = "Código inválido o expirado. Solicita un nuevo correo de recuperación." });

                // ── 2. Determinar tipo de usuario (re-buscar por email) ───────────
                bool actualizado;
                var usuarioInterno = await _usuarioAuthService.ObtenerUsuarioPorEmailAsync(request.Email);

                if (usuarioInterno != null)
                {
                    // Usuario interno: actualizar en UsuariosAuth con hash SHA-256
                    _logger.LogInformation("Cambiando contraseña de usuario interno: {Email}", request.Email);
                    actualizado = await _usuarioAuthService.ActualizarContrasenaPorEmailAsync(
                        request.Email, request.NuevoPassword);
                }
                else
                {
                    // Cliente legacy: actualizar CTextoExtra1 en AdmClientes con hash SHA-256
                    _logger.LogInformation("Cambiando contraseña de cliente legacy: {Email}", request.Email);
                    actualizado = await _usuarioAuthService.ActualizarContrasenaClienteLegacyPorEmailAsync(
                        request.Email, request.NuevoPassword);
                }

                if (!actualizado)
                    return StatusCode(500, new { message = "Error al actualizar la contraseña. Contacta al soporte." });

                _logger.LogInformation("Contraseña cambiada exitosamente para: {Email}", request.Email);

                return Ok(new { message = "Contraseña actualizada exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña de recuperación para email: {Email}", request.Email);
                return StatusCode(500, new { message = "Error interno del servidor." });
            }
        }

        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Validar el request
                if (request == null || string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { message = "Contraseña actual y nueva contraseña son requeridas" });
                }

                // Extraer el ID del usuario desde el JWT Bearer token
                var userId = await GetCurrentUserIdFromJwtAsync();

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Token de acceso inválido o no proporcionado" });
                }

                // Convertir userId string a int
                if (!int.TryParse(userId, out var userIdInt))
                {
                    return Unauthorized(new { message = "ID de usuario inválido en el token" });
                }

                // Obtener el usuario por ID
                var usuario = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(userIdInt);
                if (usuario == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                // Validar la contraseña actual usando el servicio de autenticación
                var credencialesValidas = await _usuarioAuthService.ValidarCredencialesAsync(usuario.Email, request.OldPassword);
                if (credencialesValidas == null)
                {
                    _logger.LogWarning("Intento de cambio de contraseña con contraseña actual incorrecta para usuario: {UserId}", userId);
                    return BadRequest(new { message = "Contraseña actual incorrecta" });
                }

                // Actualizar la contraseña usando el servicio
                var actualizado = await _usuarioAuthService.ActualizarContrasenaAsync(userIdInt, request.NewPassword);
                if (!actualizado)
                {
                    return StatusCode(500, new { message = "Error al actualizar la contraseña" });
                }

                _logger.LogInformation("Usuario {UserId} - {Email} cambió su contraseña exitosamente", userId, usuario.Email);

                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (FormatException)
            {
                return Unauthorized(new { message = "Token inválido" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cambiar contraseña");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Metodo para actualizar info de un usuario
        /// </summary>
        /// <param name="refreshToken"></param>
        [HttpPost("update-user")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
        {
            try
            {
                //Validar request vacio
                if (dto == null)
                    return BadRequest(new { message = "Request vacio" });

                //Actualizar Usuario
                var actualizado = await _usuarioAuthService.UpdateUserAsync(dto);
                if (!actualizado.Success)
                    return BadRequest(new { actualizado.Message });

                _logger.LogInformation("Usuario {UserId} actualizado exitosamente", dto.IdUsuario);

                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("delete-user/{idUsuario}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "ADMINISTRACION")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteUser(int idUsuario)
        {
            try
            {
                // Validar que el ID sea válido
                if (idUsuario <= 0)
                {
                    return BadRequest(new { message = "ID de usuario inválido" });
                }
                //Validar que no este enlazado el usuario
                var user = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(idUsuario);
                if (user.TieneAgenteLegacy) return BadRequest(new { message = "No se puede eliminar un usuario enlazado" });

                //Validar que no sea el mismo usuario logeado
                var userId = await GetCurrentUserIdFromJwtAsync();
                if (user.Id == int.Parse(userId))
                    return BadRequest(new { message = "No se puede eliminar el usuario logeado" });

                // Eliminar usuario
                var eliminado = await _usuarioAuthService.DeleteUserAsync(idUsuario);
                if (!eliminado.Success)
                {
                    return BadRequest(new { message = eliminado.Message });
                }

                _logger.LogInformation("Usuario {UserId} eliminado exitosamente", idUsuario);

                return Ok(eliminado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


        #region Métodos Privados de Seguridad

        private void SetRefreshTokenCookie(string refreshToken)
        {
            // ✅ MEJORA 2: Usar CookieHelper para refresh token con seguridad completa
            CookieHelper.SetSecureRefreshCookie(Response, refreshToken, expiryDays: 7);
        }

        private void SetAccessTokenCookie(string accessToken)
        {
            // ✅ MEJORA 2: Usar CookieHelper para access token con seguridad completa
            CookieHelper.SetSecureJwtCookie(Response, accessToken, expiryMinutes: 30);
        }

        private void ClearAuthCookies()
        {
            // ✅ MEJORA 2: Usar CookieHelper para eliminar cookies de forma segura
            CookieHelper.DeleteAllAuthCookies(Response);
        }

        private (string AccessToken, string RefreshToken) GenerateTokens(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = Encoding.UTF8.GetBytes(secretKey ?? throw new InvalidOperationException("JWT SecretKey not configured"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id),
                    new Claim("email", user.Email),
                    new Claim("name", user.Name),
                    new Claim("role", user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(540),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };


            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            // Refresh token simple (en producción usar algo más robusto)
            var refreshToken = Guid.NewGuid().ToString() + "_" + user.Id;

            return (accessToken, refreshToken);
        }

        private bool IsValidRefreshToken(string refreshToken)
        {
            // TODO: Validar contra BD o cache
            return !string.IsNullOrEmpty(refreshToken) && refreshToken.Contains("_");
        }

        private bool IsValidAccessToken(string accessToken)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var key = Encoding.UTF8.GetBytes(secretKey ?? throw new InvalidOperationException("JWT SecretKey not configured"));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    // Los tokens de GenerateTokens no incluyen iss/aud.
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };

                tokenHandler.ValidateToken(accessToken, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetUserIdFromRefreshToken(string refreshToken)
        {
            return refreshToken.Split('_')[1];
        }

        private string GetUserIdFromAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(accessToken);
            return token.Claims.FirstOrDefault(x => x.Type == "id")?.Value ?? string.Empty;
        }

        /// <summary>
        /// Extrae el ID del usuario autenticado desde el JWT Bearer token de la request actual
        /// </summary>
        /// <returns>ID del usuario o null si no está autenticado</returns>
        private Task<string?> GetCurrentUserIdFromJwtAsync()
        {
            try
            {
                // Primero intentar obtener el token del header Authorization Bearer
                var authHeader = Request.Headers.Authorization.FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    if (IsValidAccessToken(token))
                    {
                        return Task.FromResult<string?>(GetUserIdFromAccessToken(token));
                    }
                }

                // Si no hay Bearer token, intentar obtener de cookies (fallback)
                var cookieToken = Request.Cookies["AccessToken"];
                if (!string.IsNullOrEmpty(cookieToken) && IsValidAccessToken(cookieToken))
                {
                    return Task.FromResult<string?>(GetUserIdFromAccessToken(cookieToken));
                }

                return Task.FromResult<string?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al extraer user ID del JWT");
                return Task.FromResult<string?>(null);
            }
        }

        #endregion

        #region TODO: Métodos de Base de Datos (reemplazar con implementación real)

      

        private async Task<User?> GetUserByIdAsync(string id)
        {
            try
            {
                if (!int.TryParse(id, out var userId))
                {
                    return null;
                }

                var usuario = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(userId);
                if (usuario == null)
                {
                    return null;
                }

              

                return new User
                {
                    Id = usuario.Id.ToString(),
                    Email = usuario.Email,
                    Name = usuario.NombreCompleto,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {UserId}", id);
                return null;
            }
        }

        // Método obsoleto mantenido para compatibilidad con refresh token (será refactorizado)
        private User GetUserById(string id)
        {
            // Este método será reemplazado gradualmente por GetUserByIdAsync
            try
            {
                if (!int.TryParse(id, out var userId))
                {
                    return new User { Id = id, Email = "unknown", Name = "Unknown User", Role = "unknown", Permissions = Array.Empty<string>() };
                }

                // Por ahora devolvemos datos básicos, TODO: hacer asíncrono
                return new User
                {
                    Id = id,
                    Email = "user@temp.com",
                    Name = "Usuario Temporal",
                    Role = "user",
                    Permissions = new[] { "basic.read" }
                };
            }
            catch
            {
                return new User { Id = id, Email = "error", Name = "Error User", Role = "error", Permissions = Array.Empty<string>() };
            }
        }

        // Métodos ValidateCurrentPassword y UpdateUserPassword removidos
        // Ahora se usa _usuarioAuthService.ValidarCredencialesAsync y _usuarioAuthService.ActualizarContrasenaAsync

        #endregion
    }
}

#region DTOs

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RecaptchaToken { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
    public int? IdAgenteLegacy { get; set; }
}

#endregion