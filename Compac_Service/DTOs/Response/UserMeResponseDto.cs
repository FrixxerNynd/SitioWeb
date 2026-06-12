// =====================================================================================
// DTO RESPONSE USER ME - UserMeResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de respuesta específico para el endpoint /api/Auth/me
// Contiene información completa del usuario autenticado incluyendo permisos
//
// CUÁNDO USARLO:
// - Respuesta del endpoint GET /api/Auth/me
// - Cuando se necesita información completa del usuario actual
//
// =====================================================================================

using System.Text.Json.Serialization;

namespace CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para el endpoint /api/Auth/me con información completa del usuario
    /// </summary>
    public class UserMeResponseDto
    {
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Email del usuario
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        [JsonPropertyName("apellido")]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        [JsonPropertyName("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono del usuario
        /// </summary>
        [JsonPropertyName("telefono")]
        public long Telefono { get; set; }

        /// <summary>
        /// Rol del usuario en el sistema
        /// </summary>
        [JsonPropertyName("rol")]
        public string Rol { get; set; } = string.Empty;

        /// <summary>
        /// Permisos del usuario basados en su rol
        /// </summary>
        [JsonPropertyName("permissions")]
        public string[] Permissions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        [JsonPropertyName("activo")]
        public bool Activo { get; set; }

        /// <summary>
        /// Tipo de transmisión habilitada
        /// </summary>
        [JsonPropertyName("transmisionHabilitada")]
        public string? TransmisionHabilitada { get; set; }

        /// <summary>
        /// Indica si puede usar vehículos de empresa
        /// </summary>
        [JsonPropertyName("puedeUsarVehiculo")]
        public bool PuedeUsarVehiculo { get; set; }

        /// <summary>
        /// Fecha de creación del usuario
        /// </summary>
        [JsonPropertyName("creadoEn")]
        public DateTime CreadoEn { get; set; }

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        [JsonPropertyName("actualizadoEn")]
        public DateTime? ActualizadoEn { get; set; }

        /// <summary>
        /// ID del agente legacy asociado (si existe)
        /// </summary>
        [JsonPropertyName("idAgente")]
        public int? IdAgenteLegacy { get; set; }
    }
}
