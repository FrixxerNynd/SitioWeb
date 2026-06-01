// =====================================================================================
// DTO RESPONSE LOGIN - LoginExitosoResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de respuesta para login exitoso.
// Contiene la información que se retorna al cliente después
// de una autenticación exitosa.
//
// CUÁNDO USARLO:
// - Respuesta de login exitoso
// - Incluir token JWT y información del usuario
//
// CÓMO USARLO:
// return Ok(new LoginExitosoResponseDto 
// { 
//     Token = token,
//     Usuario = userDto
// });
//
// =====================================================================================

using System.Text.Json.Serialization;

namespace CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para login exitoso
    /// </summary>
    public class LoginExitosoResponseDto
    {
        /// <summary>
        /// Token JWT para autenticación
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Expiracion del token JWT
        /// </summary>
        /// <example>2024-01-15T22:30:00Z</example>
        [JsonPropertyName("expiracion")]
        public DateTime Expiracion { get; set; }

        /// <summary>
        /// Token de refresco para renovar la sesión
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de token (siempre Bearer)
        /// </summary>
        /// <example>Bearer</example>
        [JsonPropertyName("tipoToken")]
        public string TipoToken { get; set; } = "Bearer";

        /// <summary>
        /// Fecha de expiración del token
        /// </summary>
        /// <example>2024-01-15T22:30:00Z</example>
        [JsonPropertyName("expiraEn")]
        public DateTime ExpiraEn { get; set; }

        /// <summary>
        /// Información del usuario autenticado
        /// </summary>
        [JsonPropertyName("usuario")]
        public UsuarioResponseDto Usuario { get; set; } = new();

        /// <summary>
        /// Indica si el login fue exitoso
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("exitoso")]
        public bool Exitoso { get; set; } = true;

        /// <summary>
        /// ID del agente autenticado
        /// </summary>
        /// <example>1</example>
        [JsonPropertyName("idAgente")]
        public int? IdAgenteLegacy { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado
        /// </summary>
        /// <example>Login exitoso</example>
        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = "Login exitoso";
    }
}