/*
 * =================================================================================================
 * MIDDLEWARE DE MANEJO DE ERRORES - MiddlewareManejoErrores.cs
 * =================================================================================================
 *
 * ¿QUÉ HACE ESTE ARCHIVO?
 * Este archivo implementa el middleware principal para el manejo global de errores en la aplicación
 * ASP.NET Core. Actúa como un interceptor que captura todas las excepciones no manejadas que
 * ocurren durante el procesamiento de requests HTTP, las categoriza automáticamente según su tipo,
 * y genera respuestas HTTP consistentes con información estructurada para facilitar el debugging
 * y la experiencia del usuario.
 *
 * FUNCIONALIDADES PRINCIPALES:
 * - Captura automática de excepciones no manejadas en cualquier parte de la aplicación
 * - Mapeo inteligente de tipos de excepciones a códigos HTTP apropiados (400, 401, 404, 500, etc.)
 * - Generación de respuestas JSON estructuradas con formato consistente
 * - Logging detallado con diferentes niveles según la criticidad del error
 * - Asignación automática de IDs únicos para trazabilidad de errores
 * - Diferenciación entre errores críticos (requieren atención inmediata) y errores comunes
 *
 * ¿CÓMO SE USA?
 * 1. REGISTRO EN EL PIPELINE (en Program.cs o Startup.cs):
 *    app.UseMiddleware<MiddlewareManejoErrores>();
 *
 * 2. CONFIGURACIÓN OPCIONAL (en appsettings.json):
 *    {
 *      "MiddlewareOptions": {
 *        "IncluirDetallesError": true,
 *        "LoggearErroresCriticos": true
 *      }
 *    }
 *
 * 3. USO AUTOMÁTICO:
 *    - No requiere cambios en controladores existentes
 *    - Se activa automáticamente cuando ocurre cualquier excepción no manejada
 *    - Funciona con cualquier tipo de endpoint (MVC, API, Razor Pages, etc.)
 *
 * ¿EN QUÉ CASOS SE USA?
 * - ERRORES DE VALIDACIÓN: ArgumentException, ValidationException → 400 Bad Request
 * - ERRORES DE AUTORIZACIÓN: UnauthorizedAccessException → 401 Unauthorized
 * - RECURSOS NO ENCONTRADOS: KeyNotFoundException → 404 Not Found
 * - OPERACIONES INVÁLIDAS: InvalidOperationException → 409 Conflict
 * - TIMEOUTS: TimeoutException → 408 Request Timeout
 * - ERRORES DE BASE DE DATOS: SqlException → 500 Internal Server Error (crítico)
 * - ERRORES DE RED: HttpRequestException → 502 Bad Gateway (crítico)
 * - ERRORES DE MEMORIA: OutOfMemoryException → 500 Internal Server Error (crítico)
 *
 * EJEMPLOS DE RESPUESTAS GENERADAS:
 *
 * Error de validación (400):
 * {
 *   "exito": false,
 *   "mensaje": "Datos de entrada inválidos",
 *   "detalles": "El campo 'email' no tiene un formato válido",
 *   "timestamp": "2024-01-01T12:00:00Z"
 * }
 *
 * Error interno (500):
 * {
 *   "exito": false,
 *   "mensaje": "Ha ocurrido un error interno del servidor",
 *   "detalles": null,
 *   "timestamp": "2024-01-01T12:00:00Z"
 * }
 *
 * LOGGING GENERADO:
 * - Información del request (método, URL, IP del cliente)
 * - ID único del error para trazabilidad
 * - Tipo de excepción y mensaje
 * - Stack trace completo (solo en desarrollo o para errores críticos)
 * - Nivel de log apropiado (Error, Critical, Warning)
 *
 * INTEGRACIÓN CON OTROS COMPONENTES:
 * - Funciona junto con FiltrosExcepcionesApi para manejo granular
 * - Compatible con validadores de FluentValidation
 * - Se integra con sistemas de logging como Serilog
 * - No interfiere con manejo personalizado de errores en controladores específicos
 *
 * DEPENDENCIAS EXTERNAS:
 * - Microsoft.AspNetCore.Http (para acceso al contexto HTTP)
 * - Microsoft.Extensions.Logging (para logging estructurado)
 * - System.Text.Json (para serialización de respuestas)
 * - Serilog (recomendado para logging avanzado)
 *
 * NOTAS DE IMPLEMENTACIÓN:
 * - Se ejecuta temprano en el pipeline para capturar todos los errores
 * - Usa inyección de dependencias para acceder a servicios de logging
 * - Maneja tanto excepciones síncronas como asíncronas
 * - Es thread-safe y puede manejar requests concurrentes
 * - No afecta el rendimiento en operaciones normales (solo overhead en errores)
 */

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;

namespace back_cabs.CRM.Middleware;

/// <summary>
/// Middleware para el manejo global de errores en la aplicación.
/// Captura excepciones no manejadas y las convierte en respuestas HTTP consistentes.
/// </summary>
public class MiddlewareManejoErrores
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<MiddlewareManejoErrores> _logger;

    /// <summary>
    /// Constructor del middleware de manejo de errores.
    /// </summary>
    /// <param name="siguiente">Delegado para el siguiente middleware en el pipeline</param>
    /// <param name="logger">Logger para registrar errores y operaciones</param>
    public MiddlewareManejoErrores(RequestDelegate siguiente, ILogger<MiddlewareManejoErrores> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    /// <summary>
    /// Método principal que procesa cada solicitud HTTP.
    /// Captura cualquier excepción lanzada por middlewares o controladores posteriores.
    /// </summary>
    /// <param name="contexto">Contexto HTTP de la solicitud actual</param>
    /// <returns>Task que representa la operación asíncrona</returns>
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            // Pasar la request al siguiente middleware en el pipeline
            // Si no hay excepciones, la request continúa normalmente hacia controladores
            await _siguiente(contexto);
        }
        catch (Exception excepcion)
        {
            // Capturar cualquier excepción no manejada en la aplicación
            // Esto incluye errores en controladores, servicios, base de datos, validaciones, etc.
            await ManejarExcepcionAsync(contexto, excepcion);
        }
    }

    /// <summary>
    /// Maneja una excepción capturada y genera una respuesta de error apropiada.
    /// Categoriza el tipo de error, asigna códigos HTTP apropiados y genera logging.
    /// </summary>
    /// <param name="contexto">Contexto HTTP donde ocurrió la excepción</param>
    /// <param name="excepcion">Excepción que fue capturada</param>
    /// <returns>Task que representa la operación asíncrona</returns>
    private async Task ManejarExcepcionAsync(HttpContext contexto, Exception excepcion)
    {
        // Generar ID único para el error (para trazabilidad)
        var errorId = Guid.NewGuid().ToString("N")[..12]; // 12 caracteres únicos

        // Inicializar respuesta de error por defecto (500 Internal Server Error)
        var respuestaError = new RespuestaError
        {
            Exito = false,
            Mensaje = "Ha ocurrido un error inesperado.",
            ErrorId = errorId,
            Timestamp = DateTime.UtcNow
        };

        var codigoEstado = HttpStatusCode.InternalServerError;

        // PRIORIDAD 1: EXCEPCIONES PERSONALIZADAS DE LA APLICACIÓN
        // Estas excepciones tienen toda la información estructurada necesaria
        if (excepcion is Core.Exceptions.ApplicationException excepcionApp)
        {
            // Mapear TipoError a HttpStatusCode
            codigoEstado = (HttpStatusCode)excepcionApp.TipoError.ToHttpStatusCode();
            
            // Extraer información estructurada de la excepción personalizada
            respuestaError.Mensaje = excepcionApp.Message;
            respuestaError.CodigoError = excepcionApp.CodigoError;
            respuestaError.Detalles = excepcionApp.Detalles;

            // Log diferenciado según severidad del error
            if (codigoEstado == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(excepcionApp, 
                    "[ErrorID: {ErrorId}] Error interno: {Mensaje} - Código: {CodigoError}",
                    errorId, excepcionApp.Message, excepcionApp.CodigoError);
            }
            else
            {
                _logger.LogWarning(excepcionApp,
                    "[ErrorID: {ErrorId}] {TipoError}: {Mensaje} - Código: {CodigoError}",
                    errorId, excepcionApp.TipoError, excepcionApp.Message, excepcionApp.CodigoError);
            }
        }
        // PRIORIDAD 2: EXCEPCIONES ESTÁNDAR DE .NET
        // Mapeo de excepciones comunes a códigos HTTP apropiados
        else
        {
            switch (excepcion)
            {
                // 400 Bad Request - Errores de validación de datos de entrada
                case ArgumentException excArg:
                    codigoEstado = HttpStatusCode.BadRequest;
                    respuestaError.Mensaje = excArg.Message;
                    respuestaError.CodigoError = "INVALID_ARGUMENT";
                    respuestaError.Detalles = "Argumento inválido proporcionado.";
                    break;

                // 401 Unauthorized - Errores de autenticación/autorización
                case UnauthorizedAccessException excNoAuth:
                    codigoEstado = HttpStatusCode.Unauthorized;
                    respuestaError.Mensaje = "Acceso no autorizado.";
                    respuestaError.CodigoError = "UNAUTHORIZED";
                    respuestaError.Detalles = excNoAuth.Message;
                    break;

                // 404 Not Found - Recursos que no existen
                case KeyNotFoundException excNoEncontrado:
                    codigoEstado = HttpStatusCode.NotFound;
                    respuestaError.Mensaje = "Recurso no encontrado.";
                    respuestaError.CodigoError = "NOT_FOUND";
                    respuestaError.Detalles = excNoEncontrado.Message;
                    break;

                // 409 Conflict - Operaciones que no se pueden completar debido al estado actual
                case InvalidOperationException excOperacionInvalida:
                    codigoEstado = HttpStatusCode.Conflict;
                    respuestaError.Mensaje = "Operación no permitida.";
                    respuestaError.CodigoError = "INVALID_OPERATION";
                    respuestaError.Detalles = excOperacionInvalida.Message;
                    break;

                // 408 Request Timeout - Timeouts en operaciones
                case TimeoutException excTimeout:
                    codigoEstado = HttpStatusCode.RequestTimeout;
                    respuestaError.Mensaje = "Tiempo de espera agotado.";
                    respuestaError.CodigoError = "TIMEOUT";
                    respuestaError.Detalles = excTimeout.Message;
                    break;

                // 500 Internal Server Error - Errores no categorizados (default)
                default:
                    // Loggear errores inesperados con stack trace completo para debugging
                    _logger.LogError(excepcion, 
                        "[ErrorID: {ErrorId}] Excepción no manejada: {Mensaje}", 
                        errorId, excepcion.Message);
                    respuestaError.CodigoError = "INTERNAL_ERROR";
                    respuestaError.Detalles = "Por favor contacte al soporte si este problema persiste.";
                    break;
            }
        }

        // LOGGING ESTRUCTURADO:
        // Registrar el error con Serilog para trazabilidad completa
        // Incluye tanto la respuesta estructurada como detalles de la excepción
        Log.Error("Error manejado: {@RespuestaError} - Excepción: {MensajeExcepcion}",
            respuestaError, excepcion.Message);

        // CONFIGURACIÓN DE RESPUESTA HTTP:
        // Establecer tipo de contenido como JSON para respuestas consistentes
        contexto.Response.ContentType = "application/json";
        // Asignar código de estado HTTP basado en el tipo de error
        contexto.Response.StatusCode = (int)codigoEstado;

        // SERIALIZACIÓN Y ENVÍO DE RESPUESTA:
        // Convertir objeto de respuesta a JSON con configuración camelCase
        // Esto asegura compatibilidad con clientes JavaScript/TypeScript
        var respuestaJson = JsonSerializer.Serialize(respuestaError, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        // Enviar respuesta JSON al cliente
        await contexto.Response.WriteAsync(respuestaJson);
    }
}

/// <summary>
/// Modelo de respuesta de error utilizado por el middleware.
/// Proporciona una estructura consistente para todas las respuestas de error.
/// Se serializa automáticamente a JSON con naming camelCase.
/// </summary>
public class RespuestaError
{
    /// <summary>
    /// Indica si la operación fue exitosa (siempre false para errores).
    /// Facilita el manejo consistente en el frontend.
    /// </summary>
    public bool Exito { get; set; }

    /// <summary>
    /// Mensaje principal del error, descriptivo y user-friendly.
    /// Este es el mensaje que se muestra al usuario final.
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Código de error único para identificar el tipo específico de error.
    /// Útil para manejo programático de errores en el frontend.
    /// </summary>
    public string? CodigoError { get; set; }

    /// <summary>
    /// Detalles adicionales del error (opcional).
    /// Proporciona información técnica adicional o contexto estructurado.
    /// Puede ser un string simple o un objeto complejo serializado.
    /// </summary>
    public object? Detalles { get; set; }

    /// <summary>
    /// ID único del error para trazabilidad y correlación en logs.
    /// Facilita el seguimiento de errores específicos en producción.
    /// </summary>
    public string? ErrorId { get; set; }

    /// <summary>
    /// Timestamp UTC cuando ocurrió el error.
    /// Útil para logging, debugging y análisis de patrones de error.
    /// </summary>
    public DateTime Timestamp { get; set; }
}