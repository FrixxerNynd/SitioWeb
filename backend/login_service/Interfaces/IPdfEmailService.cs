using CRM.DTOs.Request;
using CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Shared;

/// <summary>
/// Interfaz para el servicio de envío de PDFs por correo electrónico
/// </summary>
public interface IPdfEmailService
{
    /// <summary>
    /// Envía un PDF directamente desde bytes
    /// </summary>
    /// <param name="destinatarioEmail">Correo del destinatario</param>
    /// <param name="destinatarioNombre">Nombre del destinatario (opcional)</param>
    /// <param name="asunto">Asunto del correo</param>
    /// <param name="mensajeHtml">Cuerpo del correo en HTML</param>
    /// <param name="pdfBytes">Contenido del PDF en bytes</param>
    /// <param name="nombreArchivo">Nombre del archivo PDF</param>
    /// <returns>Resultado del envío</returns>
    Task<EnviarPdfEmailResponseDto> EnviarPdfDirectoAsync(
        List<string> destinatarioEmail,
        string? destinatarioNombre,
        string asunto,
        string mensajeHtml,
        byte[] pdfBytes,
        string nombreArchivo);


    /// <summary>
    /// Genera el HTML del correo usando una plantilla predeterminada
    /// </summary>
    /// <param name="tipoDocumento">Tipo de documento (COTIZACION, ORDEN_TRABAJO, etc.)</param>
    /// <param name="variables">Variables para reemplazar en la plantilla</param>
    /// <returns>HTML del correo</returns>
    string GenerarPlantillaHtml(string tipoDocumento, Dictionary<string, string>? variables);
}