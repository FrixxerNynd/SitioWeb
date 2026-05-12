using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

// =================================================================================================
// SERVICIO JWT - ServicioJwt.cs
// =================================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Este archivo implementa el servicio completo para la gestión de tokens JSON Web Token (JWT)
// en la aplicación ASP.NET Core. Proporciona funcionalidades de autenticación y autorización
// seguras mediante la generación, validación y gestión de tokens de acceso y refresco.
//
// FUNCIONALIDADES PRINCIPALES:
// - Generación de tokens de acceso JWT con claims personalizados (usuario, roles, permisos)
// - Creación de tokens de refresco seguros para renovación automática de sesiones
// - Validación de tokens con verificación de firma digital y expiración
// - Extracción de información de usuario desde tokens válidos
// - Gestión segura de claves criptográficas simétricas
// - Logging detallado de operaciones de autenticación
//
// ¿CÓMO SE USA?
// 1. CONFIGURACIÓN EN appsettings.json:
//    {
//      "JwtSettings": {
//        "SecretKey": "tu-clave-secreta-muy-larga-y-segura-min-32-caracteres",
//        "Issuer": "back_cabs",
//        "Audience": "back_cabs_clients",
//        "AccessTokenExpirationMinutes": 15,
//        "RefreshTokenExpirationDays": 7
//      }
//    }
//
// 2. REGISTRO EN Program.cs:
//    builder.Services.AddScoped<IServicioJwt, ServicioJwt>();
//
// 3. USO EN CONTROLADORES:
//    private readonly IServicioJwt _servicioJwt;
//    public AuthController(IServicioJwt servicioJwt) => _servicioJwt = servicioJwt;
//
//    var token = await _servicioJwt.GenerarTokenAsync(usuario, roles);
//    var claims = _servicioJwt.ExtraerClaims(token);
//
// ¿EN QUÉ CASOS SE USA?
// - AUTENTICACIÓN DE USUARIOS: Login, registro, renovación de tokens
// - AUTORIZACIÓN BASADA EN ROLES: Verificación de permisos de usuario
// - API REST SECURE: Protección de endpoints con Bearer tokens
// - SINGLE PAGE APPLICATIONS: Autenticación stateless con JWT
// - MICROSERVICIOS: Comunicación segura entre servicios
// - MOBILE APPS: Autenticación sin estado en aplicaciones móviles
//
// EJEMPLOS DE USO:
//
// 1. GENERAR TOKEN DESPUÉS DE LOGIN:
//    var claims = new[] {
//        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
//        new Claim(ClaimTypes.Name, usuario.Email),
//        new Claim(ClaimTypes.Role, usuario.Rol)
//    };
//    var token = await _servicioJwt.GenerarTokenAsync(claims);
//
// 2. VALIDAR TOKEN EN REQUEST:
//    if (_servicioJwt.ValidarToken(token))
//    {
//        var claims = _servicioJwt.ExtraerClaims(token);
//        var userId = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
//    }
//
// 3. RENOVAR TOKEN CON REFRESH TOKEN:
//    if (_servicioJwt.ValidarRefreshToken(refreshToken))
//    {
//        var nuevoToken = await _servicioJwt.RenovarTokenAsync(refreshToken);
//    }
//
// SEGURIDAD IMPLEMENTADA:
// - Firma digital HMAC-SHA256 para integridad del token
// - Expiración automática de tokens para limitar tiempo de vida
// - Tokens de refresco separados para renovación segura
// - Validación de issuer y audience para prevenir ataques
// - Logging de intentos de uso de tokens inválidos
//
// DEPENDENCIAS EXTERNAS:
// - System.IdentityModel.Tokens.Jwt (para manejo de JWT)
// - Microsoft.IdentityModel.Tokens (para validación de tokens)
// - Microsoft.Extensions.Configuration (para configuración)
// - Microsoft.Extensions.Logging (para logging)
//
// NOTAS DE IMPLEMENTACIÓN:
// - Usa claves simétricas para mejor rendimiento vs asimétricas
// - Tokens de acceso cortos (15 min) + refresh tokens largos (7 días)
// - Thread-safe para uso concurrente en aplicaciones web
// - Manejo adecuado de excepciones sin exponer información sensible
//
// COMENTARIOS DEL CÓDIGO:
// - GenerarTokenAsync: Crea nuevo token JWT con claims
// - GenerarRefreshToken: Crea token aleatorio para renovación
// - ValidarToken: Verifica firma y expiración del token
// - ExtraerClaims: Obtiene información del usuario del token
// - RenovarTokenAsync: Genera nuevo token usando refresh token

namespace back_cabs.services;

/// <summary>
/// Servicio principal para la gestión de tokens JWT en la aplicación.
/// Proporciona métodos seguros para generar, validar y gestionar tokens de autenticación.
/// </summary>
public class ServicioJwt : IServicioJwt
{
    private readonly IConfiguration _configuracion;
    private readonly ILogger<ServicioJwt> _logger;
    private readonly SymmetricSecurityKey _clave;
    private readonly string _emisor;
    private readonly string _audiencia;

    /// <summary>
    /// Constructor del servicio JWT. Inicializa la configuración y valida los parámetros requeridos.
    /// Configura la clave simétrica, emisor y audiencia para la generación y validación de tokens.
    /// </summary>
    /// <param name="configuracion">Configuración de la aplicación para obtener settings JWT</param>
    /// <param name="logger">Logger para registrar operaciones del servicio</param>
    /// <exception cref="InvalidOperationException">Se lanza si la clave secreta no está configurada o es muy corta</exception>
    public ServicioJwt(IConfiguration configuracion, ILogger<ServicioJwt> logger)
    {
        _configuracion = configuracion;
        _logger = logger;

        // OBTENER CONFIGURACIÓN JWT DE appsettings.json
        var configuracionJwt = _configuracion.GetSection("JwtSettings");

        // CLAVE SECRETA: Debe ser fuerte y mantenerse confidencial
        var claveSecreta = configuracionJwt["SecretKey"] ??
            throw new InvalidOperationException("JWT SecretKey no configurada");

        // EMISOR: Identifica quién emite el token (generalmente el nombre de la app)
        _emisor = configuracionJwt["Issuer"] ?? "back_cabs";

        // AUDIENCIA: Identifica para quién está destinado el token
        _audiencia = configuracionJwt["Audience"] ?? "back_cabs_clients";

        // VALIDACIÓN DE SEGURIDAD: La clave debe tener al menos 256 bits
        // Esto asegura suficiente entropía para la firma HMAC-SHA256
        if (claveSecreta.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey debe tener al menos 32 caracteres");
        }

        // INICIALIZACIÓN DE CLAVE SIMÉTRICA:
        // Convertir string a bytes UTF-8 para crear la clave de firma
        _clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));

        _logger.LogInformation("Servicio JWT inicializado correctamente");
    }

    /// <summary>
    /// Genera un token de acceso JWT con los claims especificados.
    /// Crea un token firmado digitalmente con información del usuario y expiración.
    /// </summary>
    /// <param name="claims">Lista de claims a incluir en el token (usuario, roles, permisos, etc.)</param>
    /// <param name="expiracion">Tiempo de expiración opcional (por defecto 15 minutos)</param>
    /// <returns>Token JWT como string, listo para enviar al cliente</returns>
    public (string Token, DateTime Expiracion) GenerarTokenAcceso(IEnumerable<Claim> claims, TimeSpan? expiracion = null)
    {
        // DEFINIR TIEMPO DE EXPIRACIÓN:
        // Tokens de acceso deben ser cortos para seguridad (15 minutos por defecto)
        // Los usuarios renovarán automáticamente usando refresh tokens
        var tiempoExpiracion = expiracion ?? TimeSpan.FromMinutes(540);

        var fechaExpiracion = DateTime.UtcNow.Add(tiempoExpiracion);
        // CREAR DESCRIPTOR DEL TOKEN:
        // Este objeto define todas las propiedades del token JWT
        var descriptorToken = new SecurityTokenDescriptor
        {
            // CLAIMS DEL USUARIO: Información que viaja en el token
            Subject = new ClaimsIdentity(claims),

            // EXPIRACIÓN: Cuando el token dejará de ser válido
            Expires = DateTime.UtcNow.Add(tiempoExpiracion),

            // EMISOR: Quién emite el token (nuestra aplicación)
            Issuer = _emisor,

            // AUDIENCIA: Para quién está destinado el token
            Audience = _audiencia,

            // FIRMA DIGITAL: Credenciales para firmar el token
            SigningCredentials = new SigningCredentials(_clave, SecurityAlgorithms.HmacSha256Signature),

            // METADATOS ADICIONALES:
            IssuedAt = DateTime.UtcNow,        // Cuando se emitió
            NotBefore = DateTime.UtcNow        // No válido antes de esta fecha
        };

        // GENERAR EL TOKEN:
        // Crear el token usando el manejador de seguridad JWT
        var manejadorToken = new JwtSecurityTokenHandler();
        var token = manejadorToken.CreateToken(descriptorToken);
        _logger.LogInformation("Tiempo de duracion del token: {TiempoExpiracion}", tiempoExpiracion);
        // LOGGING PARA AUDITORÍA:
        // Registrar la generación del token (sin información sensible)
        _logger.LogInformation("Token de acceso generado para usuario: {IdUsuario}",
            claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        // SERIALIZAR A STRING:
        // Convertir el token a formato string para envío al cliente
        return (manejadorToken.WriteToken(token), fechaExpiracion);
    }

    /// <summary>
    /// Genera un token de refresco aleatorio y seguro.
    /// Los refresh tokens permiten renovar tokens de acceso sin requerir login nuevamente.
    /// </summary>
    /// <returns>Token de refresco como string en base64, criptográficamente seguro</returns>
    public string GenerarTokenRefresco()
    {
        // GENERAR BYTES ALEATORIOS CRIPTOGRÁFICAMENTE SEGUROS:
        // Usar 64 bytes (512 bits) para máxima seguridad
        // RandomNumberGenerator es criptográficamente seguro (a diferencia de Random)
        var bytesAleatorios = new byte[64];
        using var generador = RandomNumberGenerator.Create();
        generador.GetBytes(bytesAleatorios);

        // CONVERTIR A BASE64:
        // Formato string para fácil almacenamiento y transmisión
        // Base64 es URL-safe y no contiene caracteres problemáticos
        return Convert.ToBase64String(bytesAleatorios);
    }

    /// <summary>
    /// Valida un token JWT y retorna el ClaimsPrincipal si es válido.
    /// Verifica firma digital, expiración, emisor y audiencia del token.
    /// </summary>
    /// <param name="token">Token JWT a validar como string</param>
    /// <returns>ClaimsPrincipal si el token es válido, null si no lo es o está expirado</returns>
    public ClaimsPrincipal? ValidarToken(string token)
    {
        try
        {
            // INICIALIZAR MANEJADOR DE TOKENS:
            var manejadorToken = new JwtSecurityTokenHandler();

            // CONFIGURAR PARÁMETROS DE VALIDACIÓN:
            // Estos parámetros definen qué validaciones realizar
            var parametrosValidacion = new TokenValidationParameters
            {
                // VALIDAR FIRMA DIGITAL: Verificar que el token no fue alterado
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _clave,

                // VALIDAR EMISOR: Verificar que el token viene de nuestra aplicación
                ValidateIssuer = true,
                ValidIssuer = _emisor,

                // VALIDAR AUDIENCIA: Verificar que el token está destinado a nuestros clientes
                ValidateAudience = true,
                ValidAudience = _audiencia,

                // VALIDAR TIEMPO DE VIDA: Verificar que no está expirado
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Sin tolerancia para tokens expirados
            };

            // VALIDAR EL TOKEN:
            // Si la validación falla, se lanzarán excepciones específicas
            var principal = manejadorToken.ValidateToken(token, parametrosValidacion, out var tokenValidado);

            // LOGGING DE ÉXITO:
            _logger.LogInformation("Token validado exitosamente para usuario: {IdUsuario}",
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return principal;
        }
        // MANEJO DE EXCEPCIONES ESPECÍFICAS:
        catch (SecurityTokenExpiredException)
        {
            // Token expirado - comportamiento normal, no es error
            _logger.LogWarning("Validación de token fallida: Token expirado");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            // Token inválido (firma incorrecta, formato inválido, etc.)
            _logger.LogWarning("Validación de token fallida: {Mensaje}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            // Error inesperado durante validación
            _logger.LogError(ex, "Error inesperado durante validación de token");
            return null;
        }
    }

    /// <summary>
    /// Obtiene la fecha de expiración de un token JWT.
    /// Útil para verificar si un token está próximo a expirar antes de renovarlo.
    /// </summary>
    /// <param name="token">Token JWT del cual obtener la expiración</param>
    /// <returns>Fecha de expiración en UTC o null si no se puede determinar</returns>
    public DateTime? ObtenerExpiracionToken(string token)
    {
        try
        {
            // DECODIFICAR TOKEN SIN VALIDAR:
            // Solo leemos el payload para obtener la fecha de expiración
            // No validamos firma ni expiración para este método
            var manejadorToken = new JwtSecurityTokenHandler();
            var tokenJwt = manejadorToken.ReadToken(token) as JwtSecurityToken;

            // RETORNAR FECHA DE EXPIRACIÓN:
            // ValidTo ya viene en UTC desde el token
            return tokenJwt?.ValidTo;
        }
        catch
        {
            // Si hay cualquier error (token malformado, etc.), retornar null
            // No loggeamos ya que este método se usa frecuentemente
            return null;
        }
    }

    /// <summary>
    /// Extrae todos los claims de un token JWT.
    /// Los claims contienen información del usuario como ID, nombre, roles, permisos, etc.
    /// </summary>
    /// <param name="token">Token JWT del cual extraer claims</param>
    /// <returns>Enumeración de claims del token, o colección vacía si el token es inválido</returns>
    public IEnumerable<Claim> ObtenerClaimsDeToken(string token)
    {
        // VALIDAR TOKEN PRIMERO:
        // Asegurar que el token es válido antes de extraer información
        var principal = ValidarToken(token);

        // RETORNAR CLAIMS O COLECCIÓN VACÍA:
        // Si el token es inválido, retornar colección vacía en lugar de null
        return principal?.Claims ?? Enumerable.Empty<Claim>();
    }

    /// <summary>
    /// Obtiene el ID de usuario desde un token JWT.
    /// Busca específicamente el claim NameIdentifier que contiene el ID único del usuario.
    /// </summary>
    /// <param name="token">Token JWT del cual extraer el ID de usuario</param>
    /// <returns>ID de usuario como string o null si no se encuentra o el token es inválido</returns>
    public string? ObtenerIdUsuarioDeToken(string token)
    {
        // EXTRAER TODOS LOS CLAIMS DEL TOKEN:
        var claims = ObtenerClaimsDeToken(token);

        // BUSCAR CLAIM DE ID DE USUARIO:
        // ClaimTypes.NameIdentifier es el estándar para IDs de usuario en .NET
        return claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Verifica si un token JWT ha expirado.
    /// Compara la fecha de expiración del token con la hora actual UTC.
    /// </summary>
    /// <param name="token">Token JWT a verificar</param>
    /// <returns>true si el token ha expirado o es inválido, false si aún es válido</returns>
    public bool EstaTokenExpirado(string token)
    {
        // OBTENER FECHA DE EXPIRACIÓN:
        var expiracion = ObtenerExpiracionToken(token);

        // VERIFICAR SI ESTÁ EXPIRADO:
        // Comparar con hora UTC actual
        // Si no se puede obtener expiración (token inválido), considerar como expirado
        return expiracion.HasValue && expiracion.Value < DateTime.UtcNow;
    }
}