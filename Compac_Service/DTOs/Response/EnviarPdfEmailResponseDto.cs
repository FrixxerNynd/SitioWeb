namespace CRM.DTOs.Response;

/// <summary>
/// DTO de respuesta para el envío de PDF por correo electrónico
/// </summary>
public class EnviarPdfEmailResponseDto
{
    /// <summary>
    /// Indica si el envío fue exitoso
    /// </summary>
    public bool Exitoso { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora del envío (si fue exitoso)
    /// </summary>
    public DateTime? EnviadoEn { get; set; }

    /// <summary>
    /// Correo del destinatario
    /// </summary>
    public string? DestinatarioEmail { get; set; }

    /// <summary>
    /// Nombre del archivo enviado
    /// </summary>
    public string? NombreArchivo { get; set; }

    /// <summary>
    /// Código de error (si aplica)
    /// </summary>
    public string? CodigoError { get; set; }

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static EnviarPdfEmailResponseDto Exito(string destinatario, string nombreArchivo)
    {
        return new EnviarPdfEmailResponseDto
        {
            Exitoso = true,
            Mensaje = "El correo con el PDF adjunto fue enviado exitosamente",
            EnviadoEn = DateTime.UtcNow,
            DestinatarioEmail = destinatario,
            NombreArchivo = nombreArchivo
        };
    }

    /// <summary>
    /// Crea una respuesta de error
    /// </summary>
    public static EnviarPdfEmailResponseDto Error(string mensaje, string? codigoError = null)
    {
        return new EnviarPdfEmailResponseDto
        {
            Exitoso = false,
            Mensaje = mensaje,
            CodigoError = codigoError
        };
    }
}
