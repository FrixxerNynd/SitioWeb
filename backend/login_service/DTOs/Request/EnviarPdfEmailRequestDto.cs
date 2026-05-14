using System.ComponentModel.DataAnnotations;

namespace CRM.DTOs.Request;

/// <summary>
/// DTO para solicitar el envío de un PDF por correo electrónico
/// </summary>
public class EnviarPdfEmailRequestDto
{
    /// <summary>
    /// Correo electrónico del destinatario
    /// </summary>
    [Required(ErrorMessage = "El correo del destinatario es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    [MaxLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
    public List<string> DestinatarioEmail { get; set; } = new List<string>();

    /// <summary>
    /// Nombre del destinatario (opcional, para personalizar el correo)
    /// </summary>
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? DestinatarioNombre { get; set; }

    /// <summary>
    /// Asunto del correo electrónico
    /// </summary>
    [Required(ErrorMessage = "El asunto del correo es obligatorio")]
    [MaxLength(200, ErrorMessage = "El asunto no puede exceder 200 caracteres")]
    public string Asunto { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de documento (COTIZACION, ORDEN_TRABAJO, FACTURA, REPORTE)
    /// </summary>
    [Required(ErrorMessage = "El tipo de documento es obligatorio")]
    [MaxLength(20, ErrorMessage = "El tipo de documento no puede exceder 20 caracteres")]
    public string TipoDocumento { get; set; } = "COTIZACION";

    /// <summary>
    /// Variables para reemplazar en la plantilla (cliente, folio, fecha, etc.)
    /// </summary>
    public Dictionary<string, string>? Variables { get; set; }

    /// <summary>
    /// Nombre del archivo PDF (sin extensión)
    /// </summary>
    [Required(ErrorMessage = "El nombre del archivo es obligatorio")]
    [MaxLength(100, ErrorMessage = "El nombre del archivo no puede exceder 100 caracteres")]
    public string NombreArchivo { get; set; } = "documento";
}
