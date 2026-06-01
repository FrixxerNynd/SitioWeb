
// =====================================================================================
// OPCIONES DE MIDDLEWARE - OpcionesMiddleware.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define una clase de opciones para configurar el comportamiento de los middlewares
// de manejo de errores, logging y seguridad. Permite personalizar detalles de errores,
// tipos de excepciones críticas, headers personalizados y otros parámetros globales.
//
// ¿CÓMO SE USA?
// - Se instancia y configura MiddlewareOptions en Program.cs o Startup.cs
// - Se inyecta en los middlewares para modificar su comportamiento según entorno
// ¿EN QUÉ CASOS SE USA?
// - Personalizar el nivel de detalle de errores en desarrollo vs producción
// - Definir qué excepciones deben tratarse como críticas
// - Agregar headers personalizados a todas las respuestas de error
// - Ajustar timeouts y stack traces según necesidades del proyecto
//
// VENTAJAS:
// - Centraliza la configuración de middlewares
// - Facilita cambios de comportamiento sin modificar el código de los middlewares
// - Permite escenarios avanzados de logging y seguridad
// =====================================================================================
namespace back_cabs.CRM.Middleware;


/// <summary>
/// Opciones de configuración para los middlewares de la aplicación
/// </summary>
public class MiddlewareOptions
{
    /// <summary>
    /// Indica si se deben mostrar detalles de errores en las respuestas HTTP.
    /// Útil para desarrollo, pero debe estar desactivado en producción por seguridad.
    /// </summary>
    public bool IncludeErrorDetails { get; set; } = false;

    /// <summary>
    /// Lista de tipos de excepciones que se consideran críticas.
    /// Las excepciones de estos tipos se loggean con mayor severidad y pueden activar alertas.
    /// Ejemplo: errores de base de datos, timeouts, fallos de red.
    /// </summary>
    public List<Type> CriticalExceptionTypes { get; set; } = new()
    {
        typeof(Microsoft.Data.SqlClient.SqlException),
        typeof(System.Data.Common.DbException),
        typeof(System.Net.Http.HttpRequestException),
        typeof(System.TimeoutException)
    };

    /// <summary>
    /// Headers personalizados a incluir en todas las respuestas de error.
    /// Permite agregar información adicional (por ejemplo, IDs de error, trazabilidad, etc.)
    /// </summary>
    public Dictionary<string, string> CustomErrorHeaders { get; set; } = new();

    /// <summary>
    /// Tiempo de espera máximo (en segundos) para operaciones críticas.
    /// Útil para controlar timeouts en operaciones sensibles.
    /// </summary>
    public int CriticalOperationTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Indica si se debe incluir el stack trace en las respuestas de error.
    /// Solo debe activarse en desarrollo para facilitar debugging.
    /// </summary>
    public bool IncludeStackTrace { get; set; } = false;
}


/// <summary>
/// Métodos de extensión para MiddlewareOptions
/// </summary>
public static class MiddlewareOptionsExtensions
{
    /// <summary>
    /// Determina si una excepción debe considerarse crítica según la configuración
    /// </summary>
    /// <param name="options">Opciones de middleware</param>
    /// <param name="exception">Excepción a evaluar</param>
    /// <returns>True si la excepción es crítica, false en caso contrario</returns>
    public static bool IsCriticalException(this MiddlewareOptions options, Exception exception)
    {
        return options.CriticalExceptionTypes.Any(type => type.IsAssignableFrom(exception.GetType()));
    }
}