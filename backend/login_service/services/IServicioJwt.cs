using System.Security.Claims;

// =================================================================================================
// INTERFAZ SERVICIO JWT - IServicioJwt.cs
// =================================================================================================
//
// ¿QUÉ ES ESTA INTERFAZ?
// Define el contrato (métodos públicos) para el servicio de gestión de tokens JWT.
// Sigue el principio de Dependency Inversion (SOLID) al depender de abstracciones.
//
// ¿POR QUÉ USAR UNA INTERFAZ?
// 1. 🧪 TESTABILIDAD: Permite crear mocks fácilmente en tests unitarios
// 2. 🔄 FLEXIBILIDAD: Puedes cambiar la implementación sin afectar dependientes
// 3. 🎯 SOLID: Cumple con Dependency Inversion Principle
// 4. 📦 INYECCIÓN DE DEPENDENCIAS: Facilita el registro en DI container
//
// ¿CÓMO SE USA?
// En lugar de:     public UsuarioAuthService(ServicioJwt servicioJwt)
// Ahora:          public UsuarioAuthService(IServicioJwt servicioJwt)
//
// Registro en Program.cs:
// builder.Services.AddScoped<IServicioJwt, ServicioJwt>();
//
// En tests:
// var mockJwt = new Mock<IServicioJwt>();
// mockJwt.Setup(j => j.GenerarTokenAcceso(...)).Returns("fake-token");
//
// =================================================================================================

namespace back_cabs.services;

/// <summary>
/// Contrato para el servicio de gestión de tokens JWT.
/// Define las operaciones disponibles para autenticación y autorización basada en tokens.
/// </summary>
public interface IServicioJwt
{
    /// <summary>
    /// Genera un token de acceso JWT con los claims especificados.
    /// </summary>
    /// <param name="claims">Claims a incluir en el token (usuarioId, email, roles, etc.)</param>
    /// <param name="expiracion">Tiempo de expiración opcional. Si es null, usa el valor por defecto.</param>
    /// <returns>Token JWT en formato string</returns>
    /// <example>
    /// var claims = new List&lt;Claim&gt; 
    /// {
    ///     new Claim("usuarioId", "123"),
    ///     new Claim(ClaimTypes.Email, "user@example.com")
    /// };
    /// var token = servicioJwt.GenerarTokenAcceso(claims, TimeSpan.FromHours(12));
    /// </example>
    (string Token, DateTime Expiracion) GenerarTokenAcceso(IEnumerable<Claim> claims, TimeSpan? expiracion = null);

    /// <summary>
    /// Genera un token de refresco aleatorio y seguro.
    /// Se usa para renovar tokens de acceso sin requerir credenciales nuevamente.
    /// </summary>
    /// <returns>Token de refresco en formato string</returns>
    string GenerarTokenRefresco();

    /// <summary>
    /// Valida un token JWT verificando su firma, emisor, audiencia y expiración.
    /// </summary>
    /// <param name="token">Token JWT a validar</param>
    /// <returns>ClaimsPrincipal con los claims del token si es válido, null si es inválido</returns>
    ClaimsPrincipal? ValidarToken(string token);

    /// <summary>
    /// Obtiene el ID del usuario desde un token válido.
    /// Busca el claim "usuarioId" en el token.
    /// </summary>
    /// <param name="token">Token JWT del que extraer el ID</param>
    /// <returns>ID del usuario como string, o null si no se encuentra</returns>
    string? ObtenerIdUsuarioDeToken(string token);

    /// <summary>
    /// Verifica si un token ha expirado.
    /// </summary>
    /// <param name="token">Token JWT a verificar</param>
    /// <returns>True si el token está expirado, false si aún es válido</returns>
    bool EstaTokenExpirado(string token);
}
