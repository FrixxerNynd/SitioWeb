using MailKit.Net.Smtp;
using MimeKit;
using back_cabs.CRM.Interfaces.Shared;
using back_cabs.CRM.contexts;
using CRM.DTOs.Response;


namespace back_cabs.CRM.services.shared;

/// <summary>
/// Servicio para enviar PDFs adjuntos por correo electrónico usando SMTP
/// Reutiliza la configuración SmtpSettings de appsettings.json
/// ACTUALIZADO: Usa PdfMakeService en lugar de QuestPDF para generar PDFs
/// </summary>
public class PdfEmailService : IPdfEmailService
{
    private readonly ReadOnlyContext _readContext;
    private readonly WriteContext _writeContext;
    private readonly ILogger<PdfEmailService> _logger;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public PdfEmailService(
        ReadOnlyContext readContext,
        WriteContext writeContext,
        ILogger<PdfEmailService> logger,
        IConfiguration config,
        IWebHostEnvironment env)
    {
        _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
        _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }



    /// <inheritdoc />
    public async Task<EnviarPdfEmailResponseDto> EnviarPdfDirectoAsync(
        List<string> destinatariosEmail,
        string? destinatarioNombre,
        string asunto,
        string mensajeHtml,
        byte[] pdfBytes,
        string nombreArchivo)
    {
        var smtp = _config.GetSection("SmtpSettings");
        var server = smtp["Server"];
        var username = smtp["Username"];
        var password = smtp["Password"];
        var senderName = smtp["SenderName"];
        var senderEmail = smtp["SenderEmail"];

        // Evitar advertencias de nullability: usar valores por defecto basados en username
        if (string.IsNullOrEmpty(senderName)) senderName = username ?? "no-reply";
        if (string.IsNullOrEmpty(senderEmail)) senderEmail = username ?? "no-reply@localhost";

        // Configuración alternativa
        var pdfSettings = _config.GetSection("PdfEmailSettings");
        bool useAlternative = pdfSettings.GetValue<bool>("UseAlternativeSending");
        string? alternativeEmail = pdfSettings["AlternativeEmail"];

        // Validar configuración SMTP
        if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogError("Configuración SMTP incompleta en appsettings.json");
            return EnviarPdfEmailResponseDto.Error("Configuración de correo no disponible", "SMTP_CONFIG_ERROR");
        }

        try
        {
            string correoslog = string.Join(", ", destinatariosEmail);
            _logger.LogInformation("Enviando PDF a {Email}", correoslog);

            // Intentar envío al destinatario original (lanzará excepciones en caso de fallo)
            var response = await EnviarPdfInternoAsync(destinatariosEmail, destinatarioNombre, asunto, mensajeHtml, pdfBytes, nombreArchivo, server, username, password, senderName, senderEmail);
            return response;
        }
        catch (SmtpAuthenticationException authEx)
        {
            _logger.LogError(authEx, "Error de autenticación SMTP para {Email}", authEx.Email);
            return EnviarPdfEmailResponseDto.Error("Error de autenticación con el servidor de correo", "SMTP_AUTH_ERROR");
        }
        catch (SmtpSendException sendEx)
        {
            _logger.LogWarning(sendEx, "Fallo en el envío SMTP para {Email}", sendEx.Email);

            if (useAlternative && !string.IsNullOrEmpty(alternativeEmail))
            {
                _logger.LogInformation("Intentando envío alternativo a {AlternativeEmail}", alternativeEmail);
                try
                {
                    var altResult = await EnviarPdfInternoAsync(new List<string> { alternativeEmail }, "Respaldo CABS", $"[RESPALDO] {asunto}", mensajeHtml, pdfBytes, nombreArchivo, server, username, password, senderName, senderEmail);
                    altResult.Mensaje = "Enviado a email alternativo";
                    _logger.LogInformation("PDF enviado exitosamente al email alternativo {AlternativeEmail}", alternativeEmail);
                    return altResult;
                }
                catch (SmtpAuthenticationException altAuthEx)
                {
                    _logger.LogError(altAuthEx, "Autenticación SMTP alternativa fallida para {Email}", altAuthEx.Email);
                    return EnviarPdfEmailResponseDto.Error("Error de autenticación con el servidor de correo (alternativa)", "SMTP_AUTH_ERROR");
                }
                catch (SmtpSendException altSendEx)
                {
                    _logger.LogError(altSendEx, "Envío alternativo también falló para {Email}", altSendEx.Email);
                    return EnviarPdfEmailResponseDto.Error("Error al enviar el correo electrónico y alternativa fallida", "SMTP_SEND_ERROR");
                }
                catch (Exception altEx)
                {
                    _logger.LogError(altEx, "Envío alternativo falló con excepción inesperada");
                    return EnviarPdfEmailResponseDto.Error("Error al enviar el correo electrónico (alternativa)", "SMTP_SEND_ERROR");
                }
            }

            return EnviarPdfEmailResponseDto.Error("Error al enviar el correo electrónico", "SMTP_SEND_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error general al enviar PDF por correo");

            if (useAlternative && !string.IsNullOrEmpty(alternativeEmail))
            {
                _logger.LogInformation("Intentando envío alternativo a {AlternativeEmail} tras error general", alternativeEmail);
                try
                {
                    var altResult = await EnviarPdfInternoAsync(new List<string> { alternativeEmail }, "Respaldo CABS", $"[RESPALDO] {asunto}", mensajeHtml, pdfBytes, nombreArchivo, server, username, password, senderName, senderEmail);
                    altResult.Mensaje = "Enviado a email alternativo tras error";
                    _logger.LogInformation("PDF enviado exitosamente al email alternativo {AlternativeEmail}", alternativeEmail);
                    return altResult;
                }
                catch (SmtpAuthenticationException altAuthEx)
                {
                    _logger.LogError(altAuthEx, "Autenticación SMTP alternativa fallida para {Email}", altAuthEx.Email);
                    return EnviarPdfEmailResponseDto.Error("Error de autenticación con el servidor de correo (alternativa)", "SMTP_AUTH_ERROR");
                }
                catch (SmtpSendException altSendEx)
                {
                    _logger.LogError(altSendEx, "Envío alternativo también falló para {Email}", altSendEx.Email);
                    return EnviarPdfEmailResponseDto.Error("Error al enviar el correo electrónico y alternativa fallida", "SMTP_SEND_ERROR");
                }
                catch (Exception altEx)
                {
                    _logger.LogError(altEx, "Envío alternativo falló con excepción inesperada");
                }
            }

            return EnviarPdfEmailResponseDto.Error("Error al enviar el correo electrónico", "SMTP_SEND_ERROR");
        }
    }

    /// <inheritdoc />
    public string GenerarPlantillaHtml(string tipoDocumento, Dictionary<string, string>? variables)
    {
        var titulo = tipoDocumento switch
        {
            "COTIZACION" => "Cotizacion",
            "ORDEN_TRABAJO" => "Orden de Trabajo",
            "FACTURA" => "Factura",
            "REPORTE" => "Reporte",
            _ => "Documento Adjunto"
        };

        var nombreCliente = variables?.GetValueOrDefault("cliente", "Estimado cliente") ?? "Cliente";
        var folio = variables?.GetValueOrDefault("folio", "") ?? "";
        var mensajeAdicional = variables?.GetValueOrDefault("mensaje", "") ?? "";

        return $@"
<!DOCTYPE html>
<html lang='es'>

<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{titulo}</title>
    <style>
        body 
        {{font-family: 'Segoe UI', Helvetica, Arial, sans-serif;
            line-height: 1.6;
            color: #444;
            margin: 0;
            padding: 0;
        }}

        .container 
        {{max-width: 600px;
            margin: 20px auto;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            overflow: hidden;
        }}

        .header 
        {{background-color: #ffffff;
            padding: 30px;
            text-align: center;
            color: white;
            border-bottom: 5px solid #1E90FF;
        }}

        .logo 
        {{width: 250px;
            aspect-ratio: inherit;
        }}

        .img-area 
        {{
        width: 100%;
        text-align: center;
        padding: 10px 0;
        }}

        .yosoybni 
        {{
        display: block; 
        margin: 0 auto 10px auto; /* Centrado y espacio abajo */
        }}

        .yosoybni img 
        {{
            width: 160px; 
            height: auto; 
            display: inline-block;
        }}

        .marcas 
        {{
            display: block; 
            margin: 0 auto;
        }}

        .marcas img 
        {{
            width: 100%; 
            max-width: 600px; 
            height: auto; 
            display: inline-block;
        }}

        .content 
        {{padding: 30px;
            background-color: #ffffff;
        }}

        .greeting 
        {{font-size: 18px;
            font-weight: bold;
            color: #333;
        }}

        .info-box 
        {{background-color: #f8f9fa;
            border-left: 4px solid #1E90FF;
            padding: 15px;
            margin: 20px 0;
        }}

        .bank-details 
        {{width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
        }}

        .bank-details td 
        {{padding: 8px 0;
            border-bottom: 1px solid #eee;
            font-size: 14px;
        }}

        .label 
        {{font-weight: bold;
            color: #555;
            width: 120px;
        }}

        .footer 
        {{background-color: #f1f1f1;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #777;
        }}

        .footer a 
        {{color: #1E90FF;
            text-decoration: none;
        }}

        .button 
        {{display: inline-block;
            padding: 12px 25px;
            background-color: #1E90FF;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin-top: 20px;
            font-weight: bold;
        }}
    </style>
</head>

<body>
    <div class='container'>
        <div class='header'>
            <img class='logo' src='cid:logo_cabs' alt='Logo CABS'>
        </div>

        <div class='content'>
            <p class='greeting'>Estimado(a) {nombreCliente},</p>

            <p>Es un gusto saludarte. En seguimiento a tu reciente solicitud, hacemos entrega de la documentación
                correspondiente a tu cotización.</p>

            <div class='info-box'>
                <strong>Detalles del documento:</strong><br>
                Tipo: {titulo}<br>
                Folio: <span style='color: #1E90FF; font-weight: bold;'>{folio}</span>
            </div>

            {(!string.IsNullOrEmpty(mensajeAdicional) ? $"<p><em>{mensajeAdicional}</em></p>" : "")}

            <p>Adjunto a este mensaje encontrará el archivo PDF con el desglose detallado de productos y servicios.</p>

            <h3 style='border-bottom: 2px solid #1E90FF; padding-bottom: 5px; color: #333;'>Información para Pago</h3>
            <p style='font-size: 13px;'>Para procesar su pedido, puede realizar su transferencia electrónica con los
                siguientes datos:</p>

            <table class='bank-details'>
                <tr>
                    <td class='label'>BENEFICIARIO:</td>
                    <td>CABS COMPUTACION DGO SA DE CV</td>
                </tr>
                <tr>
                    <td class='label'>BANCO:</td>
                    <td>BANORTE</td>
                </tr>
                <tr>
                    <td class='label'>CUENTA:</td>
                    <td>08498985127</td>
                </tr>
                <tr>
                    <td class='label'>CLABE:</td>
                    <td>072190008498985127</td>
                </tr>
                <tr>
                    <td class='label'>RFC:</td>
                    <td>CCD 120924 V1A</td>
                </tr>
                <tr>
                    <td class='label'>TELÉFONO:</td>
                    <td>618 811 1371</td>
                </tr>
            </table>

            <p style='margin-top: 30px;'>Atentamente,<br>
                <strong>Departamento de Ventas</strong><br>
                CABS Computación
            </p>
        </div>

        <div class='img-area'>
            <div class='yosoybni'>
                <img src='cid:yosoybni' alt='Yo-soy-BNI'>
            </div>
            <div class='marcas'>
                <img src='cid:marcas' alt='Marcas'>
            </div>
        </div>

        <div class='footer'>
            <p>Este es un mensaje automático generado por el sistema CRM CABS.<br>
                Por favor, no responda directamente a este correo.</p>
            <p>© {DateTime.Now.Year} CABS Computación Durango. Todos los derechos reservados.</p>
            <p><a href='https://cabsdgo.com'>Visitar nuestro sitio web</a> </p>
        </div>
    </div>
</body>

</html>";
    }

    /// <summary>
    /// Método auxiliar para enviar PDF por SMTP
    /// </summary>
    private async Task<EnviarPdfEmailResponseDto> EnviarPdfInternoAsync(
        List<string> destinatariosEmail,
        string? destinatarioNombre,
        string asunto,
        string mensajeHtml,
        byte[] pdfBytes,
        string nombreArchivo,
        string server,
        string username,
        string password,
        string senderName,
        string senderEmail)
    {
        string correos = string.Join(", ", destinatariosEmail);
        try
        {
            // Crear mensaje
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            // --- MODIFICACIÓN PARA MÚLTIPLES DESTINATARIOS ---
            // Usamos Bcc para privacidad. 
            // Si prefieres que todos vean a todos, usa message.To.Add
            foreach (var email in destinatariosEmail.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                message.Bcc.Add(new MailboxAddress("", email.Trim()));
            }

            message.Subject = asunto;

            // Crear cuerpo con HTML y adjunto
            var bodyBuilder = new BodyBuilder();
            //Carga de Imagenes
            string webRoot = _env.WebRootPath;
            string logoPath = Path.Combine(_env.WebRootPath, "pdf-img", "CABS-Logo.png");
            string marcasPath = Path.Combine(_env.WebRootPath, "pdf-img", "marcas.png");
            string yosoybniPath = Path.Combine(_env.WebRootPath, "pdf-img", "yosoybni.png");
            if (File.Exists(logoPath))
            {
                var image = bodyBuilder.LinkedResources.Add(logoPath, new ContentType("image", "png"));
                // Este ID es el que usaremos en el <img src='cid:...'>
                image.ContentId = "logo_cabs";
                image.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
            }
            else
            {
                _logger.LogWarning("No se encontró el logo en la ruta: {Path}", logoPath);
            }
            if (File.Exists(marcasPath))
            {
                var image = bodyBuilder.LinkedResources.Add(marcasPath, new ContentType("image", "png"));
                // Este ID es el que usaremos en el <img src='cid:...'>
                image.ContentId = "marcas";
                image.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
            }
            else
            {
                _logger.LogWarning("No se encontró el logo en la ruta: {Path}", marcasPath);
            }
            if (File.Exists(yosoybniPath))
            {
                var image = bodyBuilder.LinkedResources.Add(yosoybniPath, new ContentType("image", "png"));
                // Este ID es el que usaremos en el <img src='cid:...'>
                image.ContentId = "yosoybni";
                image.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
            }
            else
            {
                _logger.LogWarning("No se encontró el logo en la ruta: {Path}", yosoybniPath);
            }

            bodyBuilder.HtmlBody = mensajeHtml;
            bodyBuilder.Attachments.Add($"{nombreArchivo}.pdf", pdfBytes, new ContentType("application", "pdf"));

            message.Body = bodyBuilder.ToMessageBody();

            // Enviar
            using var client = new SmtpClient();
            await client.ConnectAsync(server, 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return EnviarPdfEmailResponseDto.Exito(correos, $"{nombreArchivo}.pdf");
        }
        catch (MailKit.Security.AuthenticationException ex)
        {
            _logger.LogError(ex, "Error de autenticación SMTP para {correos}", correos);
            throw new SmtpAuthenticationException("Error de autenticación con el servidor de correo", ex, correos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar PDF por correo a {Email}", correos);
            throw new SmtpSendException("Error al enviar el correo electrónico", ex, correos);
        }
    }

    // Excepciones específicas para manejo más claro en EnviarPdfDirectoAsync
    public class SmtpConfigurationException : Exception
    {
        public SmtpConfigurationException(string message) : base(message) { }
    }

    public class SmtpAuthenticationException : Exception
    {
        public string Email { get; }
        public SmtpAuthenticationException(string message, Exception inner, string email) : base(message, inner)
        {
            Email = email;
        }
    }

    public class SmtpSendException : Exception
    {
        public string Email { get; }
        public SmtpSendException(string message, Exception inner, string email) : base(message, inner)
        {
            Email = email;
        }
    }
}
