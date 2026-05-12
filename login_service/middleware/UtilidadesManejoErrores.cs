// =================================================================================================
// UTILIDADES PARA MANEJO DE ERRORES - UtilidadesManejoErrores.cs
// =================================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Este archivo proporciona utilidades estáticas para el manejo centralizado de errores
// en la aplicación ASP.NET Core. Ofrece métodos helper para crear respuestas de error
// consistentes, serializar errores a JSON, identificar excepciones críticas y generar
// IDs únicos para trazabilidad.
//
using System.Text.Json;
using System.Text.Json.Serialization;

namespace back_cabs.CRM.Middleware;

public static class UtilidadesManejoErrores
{
// FUNCIONALIDADES PRINCIPALES:
// - Creación de respuestas de error estructuradas con formato consistente
// - Serialización de errores a JSON con configuración camelCase
// - Identificación automática de excepciones críticas que requieren atención inmediata
// - Generación de IDs únicos para tracking de errores
// - Obtención de mensajes de error seguros (sin información sensible)
//
// ¿CÓMO SE USA?
// 1. CREAR RESPUESTA DE ERROR:
//    var respuesta = UtilidadesManejoErrores.CreateErrorResponse(
//        TipoError.ErrorValidacion,
//        "El email no tiene formato válido",
//        "Formato esperado: usuario@dominio.com"
//    );
//
// 2. SERIALIZAR PARA RESPUESTA HTTP:
//    var json = UtilidadesManejoErrores.SerializeErrorResponse(respuesta);
//    await context.Response.WriteAsync(json);
//
// 3. VERIFICAR SI EXCEPCIÓN ES CRÍTICA:
//    if (UtilidadesManejoErrores.IsCriticalException(ex))
//        _logger.LogCritical("Error crítico detectado", ex);
//
// 4. GENERAR ID PARA TRACKING:
//    var idError = UtilidadesManejoErrores.GenerateErrorId();
//    _logger.LogError("ID Error: {IdError} - {Mensaje}", idError, ex.Message);
//
// ¿EN QUÉ CASOS SE USA?
// - EN MIDDLEWARE: Para crear respuestas de error consistentes en MiddlewareManejoErrores
// - EN FILTROS DE EXCEPCIÓN: Para generar respuestas estructuradas en FiltrosExcepcionesApi
// - EN CONTROLADORES: Para respuestas de error manuales con formato consistente
// - EN LOGGING: Para identificar excepciones críticas vs comunes
// - EN DEBUGGING: Para generar IDs únicos de error para trazabilidad
//
// EJEMPLOS PRÁCTICOS:
//
// 1. EN MIDDLEWARE DE ERRORES:
//    private async Task ManejarExcepcionAsync(HttpContext context, Exception ex)
//    {
//        var respuesta = UtilidadesManejoErrores.CreateErrorResponse(
//            TipoError.ErrorServidorInterno,
//            detalles: UtilidadesManejoErrores.GetSafeErrorMessage(ex),
//            excepcion: ex
//        );
//        // ... enviar respuesta
//    }
//
// 2. EN CONTROLADOR CON VALIDACIÓN MANUAL:
//    [HttpPost("usuarios")]
//    public IActionResult CrearUsuario([FromBody] UsuarioRequest request)
//    {
//        if (string.IsNullOrEmpty(request.Email))
//        {
//            var respuesta = UtilidadesManejoErrores.CreateErrorResponse(
//                TipoError.ErrorValidacion,
//                "El email es obligatorio"
//            );
//            return BadRequest(respuesta);
//        }
//        // ... lógica normal
//    }
//
// 3. LOGGING CON IDENTIFICACIÓN DE CRÍTICOS:
//    catch (Exception ex)
//    {
//        var idError = UtilidadesManejoErrores.GenerateErrorId();
//        var nivel = UtilidadesManejoErrores.IsCriticalException(ex)
//            ? LogLevel.Critical : LogLevel.Error;
//
//        _logger.Log(nivel, "Error ID {Id}: {Mensaje}", idError, ex.Message);
//    }
//
// INTEGRACIÓN CON OTROS COMPONENTES:
// - Usado por MiddlewareManejoErrores para crear respuestas consistentes
// - Integrado con FiltrosExcepcionesApi para manejo granular
// - Compatible con TipoError para categorización de errores
// - Trabaja con logging estructurado de Serilog
//
// VENTAJAS DE USAR ESTE SISTEMA:
// - CONSISTENCIA: Todas las respuestas de error siguen el mismo formato
// - SEGURIDAD: Los mensajes de error no exponen información sensible
// - TRAZABILIDAD: IDs únicos permiten seguimiento de errores
// - MANTENIBILIDAD: Lógica de errores centralizada en un solo lugar
// - PERFORMANCE: Métodos estáticos sin overhead de instanciación
//
// DEPENDENCIAS EXTERNAS:
// - System.Text.Json (para serialización JSON)
// - Microsoft.AspNetCore.Http (para integración con HTTP)
//
// NOTAS DE IMPLEMENTACIÓN:
// - Todos los métodos son estáticos para uso global sin instanciación
// - Thread-safe para uso concurrente en aplicaciones web
// - Manejo seguro de nulls y excepciones
// - Optimizado para rendimiento (sin reflection innecesaria)

    /// <summary>
    /// Crea una respuesta de error estructurada con formato consistente.
    /// Método principal para generar respuestas de error en toda la aplicación.
    /// </summary>
    /// <param name="tipoError">Tipo de error que determina el código HTTP y mensaje base</param>
    /// <param name="mensajePersonalizado">Mensaje personalizado opcional (reemplaza el mensaje por defecto)</param>
    /// <param name="detalles">Detalles adicionales del error para debugging</param>
    /// <param name="excepcion">Excepción original opcional para incluir información adicional</param>
    /// <returns>Objeto RespuestaError completamente configurado</returns>
    /// <example>
    /// var respuesta = UtilidadesManejoErrores.CreateErrorResponse(
    ///     TipoError.ErrorValidacion,
    ///     "El email no es válido",
    ///     "Formato esperado: usuario@dominio.com"
    /// );
    /// </example>
    public static RespuestaError CreateErrorResponse(
        TipoError tipoError,
        string? mensajePersonalizado = null,
        string? detalles = null,
        Exception? excepcion = null)
    {
        // CREACIÓN DE RESPUESTA ESTRUCTURADA:
        // Siempre establece Exito = false para errores
        // Usa mensaje personalizado o descripción por defecto del tipo de error
        // Incluye detalles adicionales si se proporcionan
        // Estampa timestamp en UTC para consistencia
        return new RespuestaError
        {
            Exito = false,
            Mensaje = mensajePersonalizado ?? tipoError.GetDescription(),
            Detalles = detalles ?? (excepcion?.Message),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Serializa una respuesta de error a formato JSON para envío HTTP.
    /// Configura opciones de serialización optimizadas para APIs REST.
    /// </summary>
    /// <param name="respuesta">Objeto RespuestaError a serializar</param>
    /// <returns>JSON string con propiedades en camelCase y sin valores null</returns>
    /// <example>
    /// var respuesta = UtilidadesManejoErrores.CreateErrorResponse(TipoError.ErrorValidacion);
    /// var json = UtilidadesManejoErrores.SerializeErrorResponse(respuesta);
    /// // Resultado: {"exito":false,"mensaje":"Los datos proporcionados no son válidos","timestamp":"2024-01-01T12:00:00Z"}
    /// </example>
    public static string SerializeErrorResponse(RespuestaError respuesta)
    {
        // SERIALIZACIÓN JSON OPTIMIZADA PARA APIs:
        // - PropertyNamingPolicy = CamelCase: Convierte PascalCase a camelCase (estándar JavaScript)
        // - WriteIndented = false: JSON compacto para mejor rendimiento de red
        // - WhenWritingNull: Omite propiedades null para respuestas más limpias
        return JsonSerializer.Serialize(respuesta, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Determina si una excepción es crítica y requiere atención inmediata del equipo.
    /// Las excepciones críticas indican problemas sistémicos que pueden afectar la estabilidad.
    /// </summary>
    /// <param name="exception">Excepción a evaluar</param>
    /// <returns>true si la excepción es crítica, false si es un error común</returns>
    /// <example>
    /// if (UtilidadesManejoErrores.IsCriticalException(ex))
    /// {
    ///     _logger.LogCritical("Error crítico del sistema detectado", ex);
    ///     // Notificar al equipo de operaciones
    /// }
    /// else
    /// {
    ///     _logger.LogError("Error de aplicación", ex);
    /// }
    /// </example>
    public static bool IsCriticalException(Exception exception)
    {
        // EVALUACIÓN DE CRITICIDAD POR TIPO DE EXCEPCIÓN:
        // Las excepciones críticas indican problemas que requieren atención inmediata:
        // - SqlException: Problemas de base de datos (conexión, consultas, etc.)
        // - DbException: Errores genéricos de base de datos
        // - HttpRequestException: Fallos en comunicaciones externas
        // - TimeoutException: Timeouts que pueden indicar cuellos de botella
        // - OutOfMemoryException: Memoria insuficiente (muy grave)
        // - StackOverflowException: Recursión infinita o stack corrupto (muy grave)
        return exception switch
        {
            // ERRORES DE BASE DE DATOS (críticos - afectan persistencia)
            Microsoft.Data.SqlClient.SqlException => true,
            System.Data.Common.DbException => true,

            // ERRORES DE RED (críticos - afectan integraciones)
            System.Net.Http.HttpRequestException => true,

            // ERRORES DE PERFORMANCE (críticos - afectan estabilidad)
            System.TimeoutException => true,

            // ERRORES DE SISTEMA (muy críticos - amenazan estabilidad)
            System.OutOfMemoryException => true,
            System.StackOverflowException => true,

            // DEFAULT: Errores comunes no críticos
            _ => false
        };
    }

    /// <summary>
    /// Obtiene un mensaje de error seguro que no expone información sensible.
    /// Traduce excepciones técnicas a mensajes user-friendly para producción.
    /// </summary>
    /// <param name="exception">Excepción original que puede contener información sensible</param>
    /// <returns>Mensaje seguro y genérico apropiado para mostrar al usuario</returns>
    /// <example>
    /// try {
    ///     // código que puede fallar
    /// } catch (Exception ex) {
    ///     var mensajeSeguro = UtilidadesManejoErrores.GetSafeErrorMessage(ex);
    ///     return BadRequest(mensajeSeguro); // No expone stack trace
    /// }
    /// </example>
    public static string GetSafeErrorMessage(Exception exception)
    {
        // TRADUCCIÓN DE EXCEPCIONES TÉCNICAS A MENSAJES USER-FRIENDLY:
        // Evita exponer información sensible como stack traces, rutas de archivos,
        // detalles de base de datos, etc. en respuestas HTTP
        return exception switch
        {
            // VALIDACIÓN: Errores de datos de entrada
            ArgumentException => "Datos de entrada inválidos",

            // AUTORIZACIÓN: Problemas de permisos
            UnauthorizedAccessException => "Acceso no autorizado",

            // RECURSOS: Elementos no encontrados
            KeyNotFoundException => "Recurso no encontrado",

            // OPERACIONES: Acciones no permitidas
            InvalidOperationException => "Operación no permitida",

            // PERFORMANCE: Timeouts
            TimeoutException => "Tiempo de espera agotado",

            // DEFAULT: Mensaje genérico para cualquier otro error
            _ => "Ha ocurrido un error interno"
        };
    }

    /// <summary>
    /// Genera un ID único para el error, útil para tracking y debugging.
    /// Crea un identificador corto y legible para correlacionar logs y errores.
    /// </summary>
    /// <returns>ID único de 8 caracteres alfanuméricos en mayúsculas</returns>
    /// <example>
    /// var idError = UtilidadesManejoErrores.GenerateErrorId();
    /// _logger.LogError("Error ID {IdError}: {Mensaje}", idError, ex.Message);
    /// // Resultado: "Error ID A1B2C3D4: Ha ocurrido un error"
    /// </example>
    public static string GenerateErrorId()
    {
        // GENERACIÓN DE ID ÚNICO PARA TRACKING:
        // Usa GUID para garantizar unicidad global
        // Formato "N": Sin guiones para mayor legibilidad
        // Substring(0, 8): Solo primeros 8 caracteres para IDs cortos
        // ToUpper(): Mayúsculas para mejor legibilidad en logs
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}