using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_cabs.CRM.Interfaces.Shared;
using CRM.DTOs.Request;
using CRM.DTOs.Response;

namespace back_cabs.CRM.controllers.shared;

/// <summary>
/// Controlador para el envío de PDFs por correo electrónico
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class PdfEmailController : ControllerBase
{
    private readonly IPdfEmailService _pdfEmailService;
    private readonly ILogger<PdfEmailController> _logger;

    public PdfEmailController(
        IPdfEmailService pdfEmailService,
        ILogger<PdfEmailController> logger)
    {
        _pdfEmailService = pdfEmailService ?? throw new ArgumentNullException(nameof(pdfEmailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Envía un PDF adjunto (pre-generado) por correo electrónico.
    /// Este endpoint espera un 'multipart/form-data' con el archivo y los datos del correo.
    /// </summary>
    /// <param name="request">Datos del correo y el archivo PDF adjunto.</param>
    /// <returns>Resultado del e    nvío.</returns>
    /// <response code="200">El correo fue enviado exitosamente.</response>
    /// <response code="400">Datos de entrada inválidos o archivo faltante.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPost("enviar")]
    [ProducesResponseType(typeof(EnviarPdfEmailResponseDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> EnviarPdf([FromBody] EnviarAdjuntoEmailRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Solicitud de envío de PDF con datos de formulario inválidos.");
            return BadRequest(new { mensaje = "Datos de entrada inválidos", errores = ModelState });
        }

        if (string.IsNullOrEmpty(request.PdfFile))
        {
            _logger.LogWarning("No se proporcionó un archivo PDF en base64 en la solicitud.");
            return BadRequest(new { mensaje = "El archivo PDF es obligatorio." });
        }

        try
        {
            _logger.LogInformation("Iniciando envío de PDF adjunto a {Email}", request.Emails);

            // Convertir base64 a byte[]
            byte[] pdfBytes;
            try
            {
                var base64Data = request.PdfFile;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }
                pdfBytes = Convert.FromBase64String(base64Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir base64 a bytes");
                return BadRequest(new { mensaje = "El archivo PDF en base64 no es válido." });
            }

            // Validar que el PDF no esté vacío
            if (pdfBytes.Length == 0)
            {
                _logger.LogWarning("El archivo PDF convertido está vacío.");
                return BadRequest(new { mensaje = "El archivo PDF no puede estar vacío." });
            }
            //Variables para la plantilla
            var variables = new Dictionary<string, string>
            {
                { "cliente", request.Nombre },
                { "folio", request.Folio },
                { "tipoDocumento", "COTIZACION" }
            };

            // Crear request para el servicio
            var serviceRequest = new EnviarPdfEmailRequestDto
            {
                DestinatarioEmail = request.Emails,
                DestinatarioNombre = request.Nombre,
                Asunto = request.Asunto,
                TipoDocumento = "COTIZACION",
                NombreArchivo = request.NombreArchivo ?? "documento"
            };

            var resultado = await _pdfEmailService.EnviarPdfDirectoAsync(
                serviceRequest.DestinatarioEmail,
                serviceRequest.DestinatarioNombre,
                serviceRequest.Asunto,
                _pdfEmailService.GenerarPlantillaHtml(serviceRequest.TipoDocumento, variables),
                pdfBytes,
                serviceRequest.NombreArchivo);

            if (resultado.Exitoso)
            {
                _logger.LogInformation("PDF adjunto enviado exitosamente a {Email}", request.Emails);
                return Ok(resultado);
            }
            else
            {
                _logger.LogWarning("Error al enviar PDF adjunto: {Mensaje}", resultado.Mensaje);
                return StatusCode((int)HttpStatusCode.InternalServerError, resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al enviar PDF adjunto");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { mensaje = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una vista previa del HTML de la plantilla de correo
    /// </summary>
    /// <param name="tipoDocumento">Tipo de documento (COTIZACION, ORDEN_TRABAJO, FACTURA, REPORTE)</param>
    /// <param name="cliente">Nombre del cliente (opcional)</param>
    /// <param name="folio">Folio del documento (opcional)</param>
    /// <returns>HTML de la plantilla</returns>
    [HttpGet("plantilla/preview")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    public IActionResult ObtenerPlantillaPreview(
        [FromQuery] string tipoDocumento = "OTRO",
        [FromQuery] string? cliente = null,
        [FromQuery] string? folio = null)
    {
        var variables = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(cliente))
            variables["cliente"] = cliente;

        if (!string.IsNullOrEmpty(folio))
            variables["folio"] = folio;

        var html = _pdfEmailService.GenerarPlantillaHtml(tipoDocumento, variables);

        return Ok(new { html, tipoDocumento });
    }

}