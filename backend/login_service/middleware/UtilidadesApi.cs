
// =====================================================================================
// UTILIDADES DE API - UtilidadesApi.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Proporciona métodos utilitarios para operaciones comunes en APIs: hashing, generación
// de tokens, sanitización, validaciones, formateo y manipulación de cadenas, entre otros.
//
// ¿CÓMO SE USA?
// - Se invocan los métodos estáticos desde controladores, servicios o middlewares
// - Facilita operaciones seguras y reutilizables en toda la aplicación
//
// ¿EN QUÉ CASOS SE USA?
// - Generar hashes seguros (contraseñas, tokens)
// - Sanitizar entradas para prevenir XSS
// - Validar URLs, formatear tamaños, truncar cadenas
// - Obtener IP real del cliente detrás de proxies
//
// VENTAJAS:
// - Centraliza utilidades comunes y seguras
// - Reduce duplicidad de código
// - Facilita mantenimiento y pruebas
// =====================================================================================
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace back_cabs.CRM.Middleware;


/// <summary>
/// Métodos utilitarios para operaciones comunes en APIs
/// </summary>
public static class ApiUtilities
{
    /// <summary>
    /// Genera un hash SHA256 de una cadena (útil para contraseñas, tokens, etc.)
    /// </summary>
    /// <param name="input">Cadena a hashear</param>
    /// <returns>Hash SHA256 en minúsculas</returns>
    public static string GenerateSha256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }


    /// <summary>
    /// Genera un token aleatorio seguro (para autenticación, recuperación, etc.)
    /// </summary>
    /// <param name="length">Longitud en bytes del token</param>
    /// <returns>Token codificado en Base64</returns>
    public static string GenerateSecureToken(int length = 32)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }


    /// <summary>
    /// Sanitiza una cadena para prevenir XSS básico (escapa caracteres peligrosos)
    /// </summary>
    /// <param name="input">Cadena a sanitizar</param>
    /// <returns>Cadena segura para mostrar en HTML</returns>
    public static string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Replace("&", "&amp;");
    }


    /// <summary>
    /// Convierte un objeto a JSON con opciones seguras (camelCase, ignora nulos)
    /// </summary>
    /// <typeparam name="T">Tipo del objeto</typeparam>
    /// <param name="obj">Objeto a serializar</param>
    /// <returns>Cadena JSON</returns>
    public static string ToJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }


    /// <summary>
    /// Verifica si una cadena contiene solo caracteres alfanuméricos y espacios
    /// </summary>
    /// <param name="input">Cadena a validar</param>
    /// <returns>True si solo contiene letras, números y espacios</returns>
    public static bool IsAlphanumericWithSpaces(string input)
    {
        return !string.IsNullOrEmpty(input) &&
               input.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c));
    }


    /// <summary>
    /// Trunca una cadena a una longitud máxima, agregando "..." si es necesario
    /// </summary>
    /// <param name="input">Cadena a truncar</param>
    /// <param name="maxLength">Longitud máxima permitida</param>
    /// <returns>Cadena truncada</returns>
    public static string Truncate(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        return input.Substring(0, maxLength - 3) + "...";
    }


    /// <summary>
    /// Genera un código alfanumérico aleatorio (útil para OTP, códigos de verificación)
    /// </summary>
    /// <param name="length">Longitud del código</param>
    /// <returns>Código alfanumérico en mayúsculas</returns>
    public static string GenerateAlphanumericCode(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }


    /// <summary>
    /// Convierte un valor de bytes a una cadena legible (B, KB, MB, GB, TB)
    /// </summary>
    /// <param name="bytes">Cantidad de bytes</param>
    /// <returns>Cadena formateada</returns>
    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }


    /// <summary>
    /// Valida si una URL es segura (debe ser HTTPS o localhost)
    /// </summary>
    /// <param name="url">URL a validar</param>
    /// <returns>True si es segura</returns>
    public static bool IsSecureUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttps || uri.Host == "localhost");
    }


    /// <summary>
    /// Obtiene la dirección IP real del cliente, considerando proxies y balanceadores
    /// </summary>
    /// <param name="context">HttpContext actual</param>
    /// <returns>IP del cliente o "unknown"</returns>
    public static string GetClientIpAddress(HttpContext context)
    {
        // Busca la IP en headers estándar de proxies
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
              ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
              ?? context.Connection.RemoteIpAddress?.ToString();

        // Si hay lista separada por comas, toma la primera (IP original)
        if (!string.IsNullOrEmpty(ip) && ip.Contains(','))
        {
            ip = ip.Split(',')[0].Trim();
        }

        return ip ?? "unknown";
    }
}