// =====================================================================================
// SERVICIO OTP HMAC - HmacOtpService.cs
// =====================================================================================
//
// ¿QUÉ HACE?
// Genera y valida códigos OTP de 6 dígitos usando HMAC-SHA256, sin almacenar
// nada en base de datos. El código se deriva de:
//   HMAC-SHA256( secretKey, email + ":" + ventana_tiempo ) → primeros 4 bytes → módulo 1,000,000
//
// VENTANA DE TIEMPO:
//   timestamp_unix / 600 = ventana de 10 minutos
//   Al validar se aceptan la ventana actual Y la anterior, dando hasta ~20 min
//   de validez total (útil si el correo llega justo al borde de la ventana).
//
// SEGURIDAD:
//   - Sin almacenamiento = sin riesgo de fuga de tokens en BD
//   - No revocable individualmente (el código "expira" solo cuando cambia la ventana)
//   - El secretKey debe mantenerse privado en appsettings / variables de entorno
//   - Si se necesita revocación explícita, combinar con un Set de códigos usados en Redis
//
// CÓMO USARLO:
//   var codigo = _otpService.GenerarCodigo(email);          // → "427831"
//   var valido  = _otpService.ValidarCodigo(email, codigo);  // → true/false
//
// =====================================================================================

using System.Security.Cryptography;
using System.Text;

namespace back_cabs.CRM.services.Auth
{
    /// <summary>
    /// Servicio OTP stateless basado en HMAC-SHA256.
    /// No requiere base de datos ni caché para funcionar.
    /// </summary>
    public class HmacOtpService
    {
        private readonly string _secretKey;
        private readonly ILogger<HmacOtpService> _logger;

        // Duración de cada ventana en segundos (600 = 10 minutos)
        private const int VentanaSegundos = 600;

        public HmacOtpService(IConfiguration configuration, ILogger<HmacOtpService> logger)
        {
            // Reutiliza el secreto JWT ya configurado, o una clave propia si existe
            _secretKey = configuration["OtpSettings:SecretKey"]
                      ?? configuration["JwtSettings:SecretKey"]
                      ?? throw new InvalidOperationException(
                             "Se requiere 'OtpSettings:SecretKey' o 'JwtSettings:SecretKey' en la configuración.");
            _logger = logger;
        }

        /// <summary>
        /// Genera un código OTP de 6 dígitos para el email dado, válido por ~10 minutos.
        /// </summary>
        /// <param name="email">Email del usuario (actúa como "claim" del código)</param>
        /// <returns>Código numérico de 6 dígitos como string, p.e. "042781"</returns>
        public string GenerarCodigo(string email)
        {
            var ventana = ObtenerVentanaActual();
            var codigo = GenerarCodigoParaVentana(email, ventana);

            _logger.LogDebug("OTP generado para {Email} en ventana {Ventana}", email, ventana);
            return codigo;
        }

        /// <summary>
        /// Valida un código OTP. Acepta la ventana actual y la anterior para tolerar
        /// correos que llegan justo al borde del intervalo de 10 minutos.
        /// </summary>
        /// <param name="email">Email del usuario al que se emitió el código</param>
        /// <param name="codigoRecibido">Código ingresado por el usuario</param>
        /// <returns>True si el código es válido y no ha expirado</returns>
        public bool ValidarCodigo(string email, string codigoRecibido)
        {
            if (string.IsNullOrWhiteSpace(codigoRecibido) || codigoRecibido.Length != 6)
                return false;

            var ventanaActual = ObtenerVentanaActual();

            // Verificar ventana actual y la inmediatamente anterior (~10 min de gracia extra)
            var codigoVentanaActual   = GenerarCodigoParaVentana(email, ventanaActual);
            var codigoVentanaAnterior = GenerarCodigoParaVentana(email, ventanaActual - 1);

            var valido = codigoVentanaActual   == codigoRecibido
                      || codigoVentanaAnterior == codigoRecibido;

            if (valido)
                _logger.LogInformation("OTP válido para {Email}", email);
            else
                _logger.LogWarning("OTP inválido o expirado para {Email}", email);

            return valido;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Métodos privados
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Retorna el número de ventana actual (unix_time / 600).</summary>
        private static long ObtenerVentanaActual()
            => DateTimeOffset.UtcNow.ToUnixTimeSeconds() / VentanaSegundos;

        /// <summary>
        /// Genera el código HMAC para una ventana específica.
        /// Mensaje = "email:ventana"  |  Clave = secretKey
        /// Código  = |primeros 4 bytes del HMAC| mod 1,000,000  → 6 dígitos con padding
        /// </summary>
        private string GenerarCodigoParaVentana(string email, long ventana)
        {
            var mensaje = $"{email.ToLower().Trim()}:{ventana}";
            var clave   = Encoding.UTF8.GetBytes(_secretKey);
            var datos   = Encoding.UTF8.GetBytes(mensaje);

            using var hmac  = new HMACSHA256(clave);
            var hash        = hmac.ComputeHash(datos);

            // Tomar los primeros 4 bytes y calcular módulo 1,000,000
            var valor  = Math.Abs(BitConverter.ToInt32(hash, 0));
            var codigo = (valor % 1_000_000).ToString("D6"); // 6 dígitos con ceros a la izquierda

            return codigo;
        }
    }
}
