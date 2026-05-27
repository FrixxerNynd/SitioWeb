using System.ComponentModel.DataAnnotations;

namespace CRM.DTOs.Request;

/// <summary>
/// DTO para solicitar el envío de un PDF adjunto por correo electrónico
/// </summary>
public class EnviarAdjuntoEmailRequestDto
{
    /// <summary>
    /// Correo electrónico del destinatario
    /// </summary>
    [Required(ErrorMessage = "El correo del destinatario es obligatorio")]
    [MaxLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
    public List<string>? Emails { get; set; }

    /// <summary>
    /// Nombre del destinatario (opcional)
    /// </summary>
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Asunto del correo electrónico
    /// </summary>
    [Required(ErrorMessage = "El asunto del correo es obligatorio")]
    [MaxLength(200, ErrorMessage = "El asunto no puede exceder 200 caracteres")]
    public string Asunto { get; set; } = string.Empty;

    /// <summary>
    /// Archivo PDF en base64
    /// </summary>
    [Required(ErrorMessage = "El archivo PDF es obligatorio")]
    public string PdfFile { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del archivo PDF (opcional, por defecto 'documento.pdf')
    /// </summary>
    [MaxLength(100, ErrorMessage = "El nombre del archivo no puede exceder 100 caracteres")]
    public string? NombreArchivo { get; set; } = "documento";

    /// <summary>
    /// Folio del documento (opcional)
    /// </summary>
    [MaxLength(100, ErrorMessage = "El folio no puede exceder 100 caracteres")]
    public string? Folio { get; set; }
}