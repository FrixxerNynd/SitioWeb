using MailKit.Net.Smtp;
using MimeKit;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using System.Text;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _env;

    public EmailService(IConfiguration config, ILogger<EmailService> logger, IWebHostEnvironment env)
    {
        _config = config;
        _logger = logger;
        _env = env;
    }

    public async Task EnviarTokenRecuperacionAsync(string destinatario, string token)
    {
        // Modo SMTP
        var smtp = _config.GetSection("SmtpSettings");
        var server = smtp["Server"];
        var username = smtp["Username"];
        var password = smtp["Password"];

        // Para testing: si son credenciales de prueba, no enviar email real, solo loguear
        if (server == "smtp.cabs_pruebas.com" && username == "Usuario" && password == "contraseña")
        {
            _logger.LogInformation($"[TEST MODE] Token de recuperación para {destinatario}: {token}");
            return;
        }

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(smtp["SenderName"], smtp["SenderEmail"]));
        mensaje.To.Add(new MailboxAddress("", destinatario));
        mensaje.Subject = "Recuperación de cuenta";

        var bodyBuilder = new BodyBuilder();

        // ── Imágenes embebidas (CID) ──────────────────────────────────────────
        AgregarImagenEmbebida(bodyBuilder, "CABS-logo.png", "logo_cabs");
        AgregarImagenEmbebida(bodyBuilder, "marcas.png",    "marcas");
        AgregarImagenEmbebida(bodyBuilder, "yosoybni.png",  "yosoybni");

        // ── Cuerpo HTML desde plantilla ───────────────────────────────────────
        bodyBuilder.HtmlBody = GenerarCuerpoHtmlRecuperacion(token, destinatario);

        mensaje.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        int port = int.TryParse(smtp["Port"], out var parsePort) ? parsePort : 587;
        try
        {
            await client.ConnectAsync(server, port, false);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(mensaje);
            await client.DisconnectAsync(true);
            _logger.LogInformation($"Correo de recuperación enviado a {destinatario} por SMTP");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar correo de recuperación a {destinatario} por SMTP");
            throw;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Agrega una imagen de wwwroot/pdf-img como recurso embebido (inline CID)
    /// igual que PdfEmailService, para que el cliente de correo la muestre sin bloquearla.
    /// </summary>
    private void AgregarImagenEmbebida(BodyBuilder bodyBuilder, string nombreArchivo, string contentId)
    {
        var path = Path.Combine(_env.WebRootPath, "pdf-img", nombreArchivo);
        if (File.Exists(path))
        {
            var image = bodyBuilder.LinkedResources.Add(path, new ContentType("image", "png"));
            image.ContentId = contentId;
            image.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
        }
        else
        {
            _logger.LogWarning("No se encontró la imagen en la ruta: {Path}", path);
        }
    }

    /// <summary>
    /// Lee EmailTemplate.html y sustituye los placeholders {token} y {destinatario}.
    /// </summary>
    private string GenerarCuerpoHtmlRecuperacion(string token, string destinatario)
    {
        // ContentRootPath apunta a la raíz del proyecto (no al bin/),
        // por lo que encontrará el archivo tanto en desarrollo como en producción.
        var templatePath = Path.Combine(
            _env.ContentRootPath, "utils", "Shared", "EmailTemplate.html");

        string html;
        if (File.Exists(templatePath))
        {
            html = File.ReadAllText(templatePath, Encoding.UTF8);
        }
        else
        {
            _logger.LogWarning("No se encontró EmailTemplate.html en: {Path}. Usando plantilla de respaldo.", templatePath);
            html = PlantillaRespaldo(token, destinatario);
        }

        // Sustituir placeholders y rutas de imágenes por CID
        html = html
            .Replace("{token}",       token)
            .Replace("{destinatario}", destinatario)
            .Replace("../../wwwroot/pdf-img/CABS-logo.png", "cid:logo_cabs")
            .Replace("../../wwwroot/pdf-img/marcas.png",    "cid:marcas")
            .Replace("../../wwwroot/pdf-img/yosoybni.png",  "cid:yosoybni");

        return html;
    }

    /// <summary>Plantilla mínima de respaldo si no se encuentra el archivo HTML.</summary>
    private static string PlantillaRespaldo(string token, string destinatario) => $@"
        <html><body>
            <p>Tu código de recuperación es: <strong>{token}</strong></p>
            <p>O haz clic aquí para cambiar tu contraseña:<br>
               <a href='https://frontend-app/cambiar-contrase%C3%B1a?email={destinatario}&token={token}'>
                   Cambiar Contraseña
               </a>
            </p>
            <p style='font-size:12px;color:#888;'>Este enlace expira en 24 horas.</p>
        </body></html>";
}
