// =====================================================================================
// DTO RESPONSE REGISTRO EXITOSO - RegistroExitosoResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la respuesta específica para un registro exitoso de usuario.
// Incluye la información del usuario creado y opcionalmente un token JWT
// para login automático después del registro.
//
// CUÁNDO USARLO:
// - Respuesta exitosa del endpoint de registro
// - Cuando se quiere incluir token para auto-login
// - Confirmación de creación de usuario
//
// CÓMO USARLO:
// return Ok(new RegistroExitosoResponseDto 
// { 
//     Usuario = usuarioDto,
//     Token = jwtToken,
//     Mensaje = "Usuario registrado exitosamente"
// });
//
// =====================================================================================

using System.Text.Json.Serialization;

namespace CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para registro exitoso de usuario
    /// </summary>
    public class RegistroExitosoResponseDto
    {
        /// <summary>
        /// Información del usuario registrado
        /// </summary>
        [JsonPropertyName("usuario")]
        public UsuarioResponseDto Usuario { get; set; } = new();
        /// <summary>
        /// Token JWT para acceso inmediato (opcional)
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        /// <summary>
        /// Token de refresco para renovar la sesión
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Mensaje de confirmación
        /// </summary>
        /// <example>Usuario registrado exitosamente</example>
        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = "Usuario registrado exitosamente";

        /// <summary>
        /// Fecha de expiración del token (si se incluye)
        /// </summary>
        /// <example>2024-01-15T11:30:00Z</example>
        [JsonPropertyName("expiraEn")]
        public DateTime? ExpiraEn { get; set; }

        /// <summary>
        /// Indica si el registro fue exitoso
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("exitoso")]
        public bool Exitoso { get; set; } = true;
    }
}