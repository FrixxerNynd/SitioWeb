
// =====================================================================================
// MODELOS DE LOGS - ModelosLog.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define modelos de datos para estructurar logs de requests, responses, errores y performance
// en el middleware. Permite logging estructurado, trazabilidad y análisis avanzado.
//
// ¿CÓMO SE USA?
// - Se instancian y llenan estos modelos en los middlewares y filtros de logging/errores
// - Se serializan a JSON para almacenar en archivos, bases de datos o sistemas de monitoreo
//
// ¿EN QUÉ CASOS SE USA?
// - Logging de requests HTTP entrantes y salientes
// - Registro de errores y excepciones con contexto
// - Auditoría de performance y operaciones lentas
// - Análisis forense y debugging avanzado
//
// VENTAJAS:
// - Estructura uniforme para todos los logs
// - Facilita integración con sistemas como Serilog, ELK, Seq, etc.
// - Permite correlacionar eventos por RequestId/ErrorId
// =====================================================================================
using System.Text.Json.Serialization;

namespace back_cabs.CRM.Middleware;


/// <summary>
/// Modelo para registrar información de cada request HTTP recibido
/// </summary>
public class RequestLogEntry
{
    /// <summary>
    /// Identificador único del request (útil para correlacionar logs)
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Método HTTP (GET, POST, etc.)
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Ruta solicitada (endpoint)
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Query string de la URL
    /// </summary>
    public string QueryString { get; set; } = string.Empty;

    /// <summary>
    /// User-Agent del cliente
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Dirección IP remota del cliente
    /// </summary>
    public string RemoteIp { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp UTC del request
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tamaño del contenido enviado (en bytes)
    /// </summary>
    public long? ContentLength { get; set; }

    /// <summary>
    /// Identificador del usuario autenticado (si aplica)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Headers HTTP relevantes
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();
}


/// <summary>
/// Modelo para registrar información de la respuesta HTTP enviada
/// </summary>
public class ResponseLogEntry
{
    /// <summary>
    /// Identificador del request asociado (para correlación)
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Código de estado HTTP devuelto (200, 404, 500, etc.)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Tamaño del contenido de la respuesta (en bytes)
    /// </summary>
    public long? ContentLength { get; set; }

    /// <summary>
    /// Duración total del procesamiento del request
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Timestamp UTC de la respuesta
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Headers HTTP relevantes de la respuesta
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();
}


/// <summary>
/// Modelo para registrar información detallada de errores y excepciones
/// </summary>
public class ErrorLogEntry
{
    /// <summary>
    /// Identificador único del error (útil para trazabilidad y soporte)
    /// </summary>
    public string ErrorId { get; set; } = UtilidadesManejoErrores.GenerateErrorId();

    /// <summary>
    /// Identificador del request asociado (si aplica)
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de error (validación, sistema, externo, etc.)
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje descriptivo del error
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Stack trace de la excepción (si está disponible)
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Ruta donde ocurrió el error
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Método HTTP asociado al error
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del usuario autenticado (si aplica)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Timestamp UTC del error
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si el error es crítico (requiere atención inmediata)
    /// </summary>
    public bool IsCritical { get; set; }

    /// <summary>
    /// Datos adicionales relevantes para el error
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}


/// <summary>
/// Modelo para registrar métricas de performance de operaciones críticas
/// </summary>
public class PerformanceLogEntry
{
    /// <summary>
    /// Identificador del request asociado (si aplica)
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la operación medida (consulta, proceso, etc.)
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Duración de la operación
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Uso de memoria en bytes durante la operación
    /// </summary>
    public long MemoryUsage { get; set; }

    /// <summary>
    /// Timestamp UTC de la métrica
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si la operación fue lenta (según umbral definido)
    /// </summary>
    public bool IsSlow { get; set; }

    /// <summary>
    /// Identificador del usuario autenticado (si aplica)
    /// </summary>
    public string? UserId { get; set; }
}


/// <summary>
/// Métodos de extensión para serializar modelos de log a JSON
/// </summary>
public static class LogEntryExtensions
{
    /// <summary>
    /// Serializa un RequestLogEntry a JSON (ignora nulos)
    /// </summary>
    public static string ToJson(this RequestLogEntry entry)
    {
        return System.Text.Json.JsonSerializer.Serialize(entry, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Serializa un ResponseLogEntry a JSON (ignora nulos)
    /// </summary>
    public static string ToJson(this ResponseLogEntry entry)
    {
        return System.Text.Json.JsonSerializer.Serialize(entry, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Serializa un ErrorLogEntry a JSON (ignora nulos)
    /// </summary>
    public static string ToJson(this ErrorLogEntry entry)
    {
        return System.Text.Json.JsonSerializer.Serialize(entry, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Serializa un PerformanceLogEntry a JSON (ignora nulos)
    /// </summary>
    public static string ToJson(this PerformanceLogEntry entry)
    {
        return System.Text.Json.JsonSerializer.Serialize(entry, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }
}