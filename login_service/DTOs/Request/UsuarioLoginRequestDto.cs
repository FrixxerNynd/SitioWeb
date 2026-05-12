// =====================================================================================
// DTO REQUEST LOGIN USUARIO - UsuarioLoginRequestDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de entrada para el endpoint de login de usuarios.
// Contiene las credenciales necesarias que el cliente debe enviar
// para autenticarse en el sistema.
//
// CUÁNDO USARLO:
// - Endpoint POST /api/auth/login
// - Validación de credenciales
// - Documentación Swagger de request
//
// CÓMO USARLO:
// [HttpPost("login")]
// public async Task<IActionResult> Login([FromBody] UsuarioLoginRequestDto request)
// {
//     // Procesar login
// }
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CRM.DTOs.Request
{
    /// <summary>
    /// DTO para solicitud de login de usuario
    /// </summary>
    public class UsuarioLoginRequestDto
    {
        /// <summary>
        /// Email del usuario para autenticación
        /// </summary>
        /// <example>juan.perez@empresa.com</example>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        /// <example>MiContraseña123!</example>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        /// <summary>
        /// Indica si se desea mantener la sesión activa
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("recordarme")]
        public bool RecordarMe { get; set; } = false;
    }
}