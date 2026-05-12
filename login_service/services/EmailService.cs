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
    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }
    public async Task EnviarTokenRecuperacionAsync(string destinatario, string token)
    {
        var gmailSettings = _config.GetSection("GmailSettings");
        bool useGmailApi = gmailSettings.GetValue<bool>("UseGmailApi");

        if (useGmailApi)
        {
            await EnviarEmailViaGmailApiAsync(destinatario, "Recuperación de cuenta", GenerarCuerpoHtmlRecuperacion(token, destinatario));
            return;
        }

        // Modo SMTP existente
        var smtp = _config.GetSection("SmtpSettings");
        var server = smtp["Server"];
        var username = smtp["Username"];
        var password = smtp["Password"];

        // Para testing: si son credenciales de prueba, no enviar email real, solo loguear
        if (server == "smtp.cabs_pruebas.com" && username == "Usuario" && password == "contraseña")
        {
            _logger.LogInformation($"[TEST MODE] Token de recuperación para {destinatario}: {token}");
            return; // No enviar email real
        }

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(smtp["SenderName"], smtp["SenderEmail"]));
        mensaje.To.Add(new MailboxAddress("", destinatario));
        mensaje.Subject = "Recuperación de cuenta";
        var htmlBody = GenerarCuerpoHtmlRecuperacion(token, destinatario);
        mensaje.Body = new TextPart("html") { Text = htmlBody };
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

    private string GenerarCuerpoHtmlRecuperacion(string token, string destinatario)
    {
        
        return $@"
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Recuperación de Contraseña - CRM Cabs</title>
            <style>
                body {{
                    margin: 0;
                    padding: 0;
                    background-color: #f4f4f4;
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    line-height: 1.6;
                    color: #333;
                }}
                .container {{
                    max-width: 600px;
                    margin: 20px auto;
                    background-color: #ffffff;
                    border-radius: 10px;
                    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                    overflow: hidden;
                }}
                .header {{
                    background: linear-gradient(135deg, #768ceb 0%, #4c52a1 100%);
                    padding: 30px 20px;
                    text-align: center;
                    color: white;
                }}
                .logo {{
                    max-width: 120px;
                    height: auto;
                    margin-bottom: 15px;
                }}
                .content {{
                    padding: 40px 30px;
                    text-align: center;
                }}
                .title {{
                    font-size: 28px;
                    font-weight: bold;
                    margin-bottom: 10px;
                    color: #22364a;
                }}
                .subtitle {{
                    font-size: 16px;
                    color: #e1e1e1;
                    margin-bottom: 30px;
                }}
                .token-box {{
                    background-color: #ecf0f1;
                    border: 2px dashed #0a6099;
                    border-radius: 8px;
                    padding: 20px;
                    margin: 20px 0;
                    display: inline-block;
                }}
                .token {{
                    font-size: 24px;
                    font-weight: bold;
                    color: #e56758;
                    letter-spacing: 2px;
                    font-family: 'Courier New', monospace;
                }}
                .button {{
                    display: inline-block;
                    background: linear-gradient(135deg, #98a8f0 0%, #502ce0 100%);
                    color: white;
                    text-decoration: none;
                    padding: 15px 30px;
                    border-radius: 25px;
                    font-weight: bold;
                    font-size: 16px;
                    margin: 20px 0;
                    box-shadow: 0 4px 15px rgba(48, 76, 203, 0.4);
                    transition: all 0.3s ease;
                }}
                .button:hover {{
                    transform: translateY(-2px);
                    box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
                }}
                .footer {{
                    background-color: #34495e;
                    color: white;
                    padding: 20px;
                    text-align: center;
                    font-size: 12px;
                }}
                .footer a {{
                    color: #3498db;
                    text-decoration: none;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <img src='./Logo_CABS.png' alt='Logo CABS' class='logo'>
                    <div class='title'>¿Olvidaste tu contraseña?</div>
                    <div class='subtitle'>Recupera el acceso a tu cuenta de forma segura</div>
                </div>
                <div class='content'>
                    <h2>Recuperación de cuenta</h2>
                    <p>Tu código de recuperación es:</p>
                    <div class='token-box'>
                        <span class='token'>{token}</span>
                    </div>
                    <p>O haz clic en el siguiente botón para cambiar tu contraseña:</p>
                    <a href='https://frontend-app/cambiar-contraseña?email={destinatario}&token={token}' class='button'>Cambiar Contraseña</a>
                    <p style='margin-top: 30px; font-size: 14px; color: #7f8c8d;'>
                        Este enlace expirará en 24 horas por seguridad.<br>
                        Si no solicitaste este cambio, ignora este mensaje.
                    </p>
                </div>
                <div class='footer'>
                    <p>Este es un mensaje automatizado del sistema CRM ©Cabs-durango</p>
                    <p>¿Necesitas ayuda? <a href='mailto:soporte@cabs-durango.com'>Contáctanos</a></p>
                </div>
            </div>
        </body>
        </html>
        
        ";
    }


    private async Task EnviarEmailViaGmailApiAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var gmailSettings = _config.GetSection("GmailSettings");
        var clientId = gmailSettings["ClientId"];
        var clientSecret = gmailSettings["ClientSecret"];
        var refreshToken = gmailSettings["RefreshToken"];
        var senderEmail = gmailSettings["SenderEmail"];

        // Crear credenciales OAuth 2.0
        var credential = GoogleCredential.FromAccessToken("") // Necesitamos obtener el access token
            .CreateScoped(GmailService.Scope.GmailSend);

        // Para simplificar, usaremos refresh token directamente
        var credentials = new UserCredential(
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            }),
            "user",
            new TokenResponse { RefreshToken = refreshToken }
        );

        // Crear servicio de Gmail
        var service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentials,
            ApplicationName = "CRM Cabs"
        });

        // Crear mensaje
        var mensaje = new Message();
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("CRM Cabs", senderEmail));
        mimeMessage.To.Add(new MailboxAddress("", destinatario));
        mimeMessage.Subject = asunto;
        mimeMessage.Body = new TextPart("html") { Text = cuerpoHtml };

        // Convertir a formato RFC 2822
        using var stream = new MemoryStream();
        await mimeMessage.WriteToAsync(stream);
        var rawMessage = Convert.ToBase64String(stream.ToArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

        mensaje.Raw = rawMessage;

        // Enviar email
        await service.Users.Messages.Send(mensaje, "me").ExecuteAsync();
    }

    public async Task EnviarPdfPorEmailAsync(string destinatario, string asunto, string nombre, byte[] pdfBytes, string nombreArchivo)
    {
        var gmailSettings = _config.GetSection("GmailSettings");
        bool useGmailApi = gmailSettings.GetValue<bool>("UseGmailApi");

        if (useGmailApi)
        {
            await EnviarPdfViaGmailApiAsync(destinatario, asunto, nombre, pdfBytes, nombreArchivo);
            return;
        }

        // Modo SMTP
        var smtp = _config.GetSection("SmtpSettings");
        var server = smtp["Server"];
        var username = smtp["Username"];
        var password = smtp["Password"];

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(smtp["SenderName"], smtp["SenderEmail"]));
        mensaje.To.Add(new MailboxAddress(nombre, destinatario));
        mensaje.Subject = asunto;

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = $"<p>Hola {nombre},</p><p>Adjunto la cotización solicitada.</p><p>Saludos,<br>Equipo CABS</p>";
        bodyBuilder.Attachments.Add(nombreArchivo, pdfBytes, new ContentType("application", "pdf"));

        mensaje.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        int port = int.TryParse(smtp["Port"], out var parsePort) ? parsePort : 587;
        try
        {
            await client.ConnectAsync(server, port, false);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(mensaje);
            await client.DisconnectAsync(true);
            _logger.LogInformation($"PDF enviado por email a {destinatario} por SMTP");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar PDF por email a {destinatario} por SMTP");
            throw;
        }
    }

    private async Task EnviarPdfViaGmailApiAsync(string destinatario, string asunto, string nombre, byte[] pdfBytes, string nombreArchivo)
    {
        var gmailSettings = _config.GetSection("GmailSettings");
        var clientId = gmailSettings["ClientId"];
        var clientSecret = gmailSettings["ClientSecret"];
        var refreshToken = gmailSettings["RefreshToken"];
        var senderEmail = gmailSettings["SenderEmail"];

        var credentials = new UserCredential(
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            }),
            "user",
            new TokenResponse { RefreshToken = refreshToken }
        );

        var service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentials,
            ApplicationName = "CRM Cabs"
        });

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("CRM Cabs", senderEmail));
        mimeMessage.To.Add(new MailboxAddress(nombre, destinatario));
        mimeMessage.Subject = asunto;

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = $"<p>Hola {nombre},</p><p>Adjunto la cotización solicitada.</p><p>Saludos,<br>Equipo CABS</p>";
        bodyBuilder.Attachments.Add(nombreArchivo, pdfBytes, new ContentType("application", "pdf"));

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using var stream = new MemoryStream();
        await mimeMessage.WriteToAsync(stream);
        var rawMessage = Convert.ToBase64String(stream.ToArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

        var mensaje = new Message { Raw = rawMessage };

        await service.Users.Messages.Send(mensaje, "me").ExecuteAsync();
        _logger.LogInformation($"PDF enviado por email a {destinatario} via Gmail API");
    }
}

