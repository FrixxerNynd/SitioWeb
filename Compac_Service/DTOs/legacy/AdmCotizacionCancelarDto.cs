using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para cancelar una cotización existente
    /// </summary>
    public class AdmCotizacionCancelarDto
    {
        /// <summary>
        /// ID del documento a cancelar (obligatorio)
        /// </summary>
        [Required(ErrorMessage = "El ID del documento es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del documento debe ser mayor a 0")]
        public int IdDocumento { get; set; }

        /// <summary>
        /// Motivo de la cancelación (opcional)
        /// </summary>
        [StringLength(254, ErrorMessage = "El motivo no puede exceder 254 caracteres")]
        public string? Motivo { get; set; }

        /// <summary>
        /// Usuario que cancela (opcional, se usa el del sistema si no se envía)
        /// </summary>
        [StringLength(15, ErrorMessage = "El usuario no puede exceder 15 caracteres")]
        public string? UsuarioCancela { get; set; }
    }

    /// <summary>
    /// DTO de respuesta al cancelar una cotización
    /// </summary>
    public class AdmCotizacionCancelarResponseDto
    {
        /// <summary>
        /// ID del documento cancelado
        /// </summary>
        public int IdDocumento { get; set; }

        /// <summary>
        /// Serie del documento
        /// </summary>
        public string Serie { get; set; } = string.Empty;

        /// <summary>
        /// Folio del documento
        /// </summary>
        public double Folio { get; set; }

        /// <summary>
        /// Razón social del cliente
        /// </summary>
        public string RazonSocial { get; set; } = string.Empty;

        /// <summary>
        /// Total de la cotización cancelada
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Fecha de cancelación
        /// </summary>
        public DateTime FechaCancelacion { get; set; }

        /// <summary>
        /// Motivo de cancelación
        /// </summary>
        public string? Motivo { get; set; }

        /// <summary>
        /// Usuario que canceló
        /// </summary>
        public string Usuario { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje informativo
        /// </summary>
        public string Mensaje { get; set; } = "Cotización cancelada exitosamente";
    }
}
