// =====================================================================================
// DTO REQUEST REGISTRO USUARIO - UsuarioRegistroRequestDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de entrada para el endpoint de registro de usuarios.
// Contiene todas las propiedades necesarias que el cliente debe enviar
// para crear un nuevo usuario en el sistema.
//
// CUÁNDO USARLO:
// - Endpoint POST /api/auth/registro
// - Validación de datos de entrada
// - Mapeo a entidad de dominio
// - Documentación Swagger de request
//
// CÓMO USARLO:
// [HttpPost("registro")]
// public async Task<IActionResult> Registrar([FromBody] UsuarioRegistroRequestDto request)
// {
//     // Procesar registro
// }
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using back_cabs.CRM.Core.Validation;
using back_cabs.CRM.DTOs.Legacy;
using Microsoft.AspNetCore.Http;

namespace CRM.DTOs.Request
{
    /// <summary>
    /// DTO para solicitud de registro de nuevo usuario
    /// </summary>
    public class UsuarioRegistroRequestDto
    {
        /// <summary>
        /// Nombre del usuario
        /// </summary>
        /// <example>Juan Carlos</example>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        /// <example>Pérez García</example>
        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [JsonPropertyName("apellido")]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono del usuario
        /// </summary>
        /// <example>5512345678</example>
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Range(0, 999999999998, ErrorMessage = "El teléfono debe ser un número válido de 10 dígitos")]
        [JsonPropertyName("telefono")]
        public long Telefono { get; set; }

        /// <summary>
        /// Email único del usuario (será usado para login)
        /// </summary>
        /// <example>juan.perez@empresa.com</example>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario (mínimo 8 caracteres)
        /// </summary>
        /// <example>MiContraseña123!</example>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de contraseña (debe coincidir con contraseña)
        /// </summary>
        /// <example>MiContraseña123!</example>
        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        [JsonPropertyName("confirmarContrasena")]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el usuario está activo (puede usar licencia de conducir)
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("activo")]
        public bool? Activo { get; set; } = true;

        /// <summary>
        /// ID Cliente (OBLIGATORIO para crear usuarios en el sitio web, se asignará al cliente registrado)
        /// </summary>
        [Required(ErrorMessage = "El ID del cliente es obligatorio para usuarios registrados desde el sitio web")]
        [JsonPropertyName("idCliente")]
        public int IdCliente { get; set; }
    }
    /// <summary>
    /// DTO para solicitud de actualizacion de usuario
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        /// <example>Juan Carlos</example>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        /// <example>Pérez García</example>
        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [JsonPropertyName("apellido")]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono del usuario
        /// </summary>
        /// <example>5512345678</example>
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Range(0, 999999999998, ErrorMessage = "El teléfono debe ser un número válido de 10 dígitos")]
        [JsonPropertyName("telefono")]
        public long Telefono { get; set; }

        /// <summary>
        /// Email único del usuario (será usado para login)
        /// </summary>
        /// <example>juan.perez@empresa.com</example>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el usuario está activo (puede usar licencia de conducir)
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("activo")]
        public bool? Activo { get; set; } = true;
    }

    public class UserChangePasswordRequestDto
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Contraseña actual del usuario
        /// </summary>
        /// <example>MiContraseña123!</example>
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [JsonPropertyName("contrasenaActual")]
        public string ContrasenaActual { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña del usuario (mínimo 8 caracteres)
        /// </summary>
        /// <example>NuevaContraseña123!</example>
        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La nueva contraseña debe tener entre 8 y 100 caracteres")]
        [JsonPropertyName("nuevaContrasena")]
        public string NuevaContrasena { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la nueva contraseña (debe coincidir con la nueva contraseña)
        /// </summary>
        /// <example>NuevaContraseña123!</example>
        [Required(ErrorMessage = "La confirmación de la nueva contraseña es obligatoria")]
        [Compare("NuevaContrasena", ErrorMessage = "Las nuevas contraseñas no coinciden")]
        [JsonPropertyName("confirmarNuevaContrasena")]
        public string ConfirmarNuevaContrasena { get; set; } = string.Empty;
    }

    public class UserClientRequestDto
    {

        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        [Required(ErrorMessage = "El RFC es obligatorio")]
        public string RFC { get; set; } = string.Empty;
        [Required(ErrorMessage = "El CURP es obligatorio")]
        public string CURP { get; set; } = string.Empty;
        [Required(ErrorMessage = "El Teléfono es obligatorio")]
        public string Telefono { get; set; } = string.Empty;
        [Required(ErrorMessage = "El Email es obligatorio")]
        public string Email { get; set; } = string.Empty;
        public string? Email2 { get; set; }
        public string? Email3 { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Contraseña { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string RecaptchaToken { get; set; } = string.Empty;
        public UbicacionDetalleDto? Direccion { get; set; }
        public IFormFile? ConstanciaFiscal { get; set; }
    }

}
