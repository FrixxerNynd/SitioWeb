using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// =================================================================================================
// FILTROS DE EXCEPCIONES API - FiltrosExcepcionesApi.cs
// =================================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Este archivo implementa filtros de excepciones específicos para APIs ASP.NET Core.
// Proporciona manejo granular de errores a nivel de controlador y acción, complementando
// el middleware global de manejo de errores. Los filtros permiten personalizar el manejo
// de excepciones específicas según el contexto de la API.
//
// FUNCIONALIDADES PRINCIPALES:
// - ApiExceptionFilterAttribute: Maneja excepciones generales no capturadas en controladores
// - ValidationExceptionFilterAttribute: Maneja errores de validación de FluentValidation
// - Logging detallado con IDs de error para trazabilidad
// - Respuestas JSON estructuradas consistentes con el resto del sistema
// - Diferenciación entre errores críticos y comunes
//
// ¿CÓMO SE USA?
// 1. A NIVEL GLOBAL (en Program.cs):
//    builder.Services.AddControllers(options =>
//    {   
//        options.Filters.Add<ApiExceptionFilterAttribute>();
//        options.Filters.Add<ValidationExceptionFilterAttribute>();
//    });
//
// 2. A NIVEL DE CONTROLADOR:
//    [ApiExceptionFilter]
//    public class MiController : ControllerBase { ... }
//
// 3. A NIVEL DE ACCIÓN:
//    [HttpPost]
//    [ValidationExceptionFilter]
//    public IActionResult CrearEntidad([FromBody] CrearEntidadRequest request) { ... }
//
// ¿EN QUÉ CASOS SE USA?
// - EXCEPCIONES NO MANEJADAS: Cualquier excepción que escape del try/catch en controladores
// - ERRORES DE VALIDACIÓN: Excepciones ValidationException de FluentValidation
// - LOGGING GRANULAR: Cuando se necesita logging específico por controlador/acción
// - RESPUESTAS PERSONALIZADAS: Cuando se requiere manejo especial para ciertos tipos de error
// - DEBUGGING: Para identificar exactamente dónde ocurren los errores en el código
//
// EJEMPLOS DE USO:
//
// 1. CONTROLADOR CON FILTROS GLOBALES:
//    [ApiController]
//    [Route("api/[controller]")]
//    public class UsuariosController : ControllerBase
//    {
//        [HttpPost]
//        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioRequest request)
//        {
//            // Si ocurre cualquier excepción no manejada, ApiExceptionFilter la captura
//            var usuario = await _servicio.CrearUsuario(request);
//            return CreatedAtAction(nameof(ObtenerUsuario), new { id = usuario.Id }, usuario);
//        }
//    }
//
// 2. ACCIÓN CON VALIDACIÓN:
//    [HttpPost("validar")]
//    [ValidationExceptionFilter] // Solo este endpoint usa el filtro de validación
//    public IActionResult ValidarDatos([FromBody] DatosRequest request)
//    {
//        // Los errores de validación serán manejados por ValidationExceptionFilter
//        return Ok("Datos válidos");
//    }
//
// INTEGRACIÓN CON OTROS COMPONENTES:
// - Funciona junto con MiddlewareManejoErrores para manejo en capas
// - Usa UtilidadesManejoErrores para crear respuestas consistentes
// - Se integra con TipoError para categorización de errores
// - Compatible con logging estructurado de Serilog
//
// DIFERENCIAS CON MIDDLEWARE:
// - Filtros: Se ejecutan después del middleware, a nivel de MVC
// - Middleware: Captura errores en toda la pipeline HTTP
// - Filtros: Permiten personalización por controlador/acción
// - Middleware: Manejo global automático sin configuración adicional
//
// DEPENDENCIAS EXTERNAS:
// - Microsoft.AspNetCore.Mvc.Filters (para ExceptionFilterAttribute)
// - Microsoft.Extensions.Logging (para logging)
// - FluentValidation (para ValidationException)
// - System.Text.Json (para serialización)
//
// NOTAS DE IMPLEMENTACIÓN:
// - Los filtros se ejecutan en orden de registro
// - Pueden marcar excepciones como manejadas para evitar reprocesamiento
// - Thread-safe para uso concurrente
// - No afectan el rendimiento en operaciones normales

namespace back_cabs.CRM.Middleware;

/// <summary>
/// Filtro de excepciones para APIs que maneja errores no capturados en controladores.
/// Proporciona logging detallado y respuestas de error consistentes.
/// Se puede aplicar a nivel de clase (controlador) o método (acción).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilterAttribute> _logger;

    /// <summary>
    /// Constructor que inyecta el logger para registro de errores.
    /// El logger se usa para registrar excepciones con diferentes niveles de severidad.
    /// </summary>
    /// <param name="logger">Instancia de logger inyectada por el contenedor de dependencias</param>
    public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Método principal que se ejecuta cuando ocurre una excepción en un controlador.
    /// Procesa la excepción, crea logging detallado y establece una respuesta HTTP estructurada.
    /// </summary>
    /// <param name="context">Contexto de la excepción con información detallada del error y la solicitud</param>
    public override void OnException(ExceptionContext context)
    {
        // CREAR RESPUESTA DE ERROR ESTRUCTURADA:
        // Utiliza las utilidades centralizadas para mantener consistencia en todas las respuestas de error
        var respuestaError = UtilidadesManejoErrores.CreateErrorResponse(
            TipoError.ErrorServidorInterno,
            detalles: UtilidadesManejoErrores.GetSafeErrorMessage(context.Exception),
            excepcion: context.Exception);

        // GENERAR ID ÚNICO PARA TRAZABILIDAD:
        // Cada error tiene un ID único que permite rastrearlo en logs distribuidos
        var idError = UtilidadesManejoErrores.GenerateErrorId();
        _logger.LogError("ID Error: {IdError} - {Excepcion}", idError, context.Exception.Message);

        // LOGGING DETALLADO CON CONTEXTO:
        // Registra el error con información del controlador y acción donde ocurrió
        _logger.LogError(context.Exception,
            "Error en {Controlador}.{Accion}: {Mensaje}",
            context.RouteData.Values["controller"],
            context.RouteData.Values["action"],
            context.Exception.Message);

        // EVALUACIÓN DE CRITICIDAD:
        // Los errores críticos requieren atención inmediata del equipo de desarrollo
        if (UtilidadesManejoErrores.IsCriticalException(context.Exception))
        {
            _logger.LogCritical(context.Exception,
                "ERROR CRÍTICO: Sistema potencialmente inestable. Revisar inmediatamente.");
        }

        // CONFIGURACIÓN DE RESPUESTA HTTP:
        // Establece el resultado de la acción con código 500 y respuesta JSON estructurada
        context.Result = new ObjectResult(respuestaError)
        {
            StatusCode = 500
        };

        // MARCAR EXCEPCIÓN COMO MANEJADA:
        // Evita que otros filtros o middleware procesen esta excepción nuevamente
        context.ExceptionHandled = true;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidationExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        // VERIFICACIÓN DEL TIPO DE EXCEPCIÓN:
        // Solo procesa excepciones de validación de FluentValidation
        // Si no es una ValidationException, permite que otros filtros la manejen
        if (context.Exception is FluentValidation.ValidationException validationException)
        {
            // AGRUPACIÓN DE ERRORES POR PROPIEDAD:
            // Organiza los errores de validación por nombre de propiedad
            // Cada propiedad puede tener múltiples errores de validación
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            // CREACIÓN DE RESPUESTA DE ERROR ESTRUCTURADA:
            // Utiliza la utilidad centralizada para crear respuesta consistente
            // Incluye el tipo de error específico para validaciones
            var respuestaError = UtilidadesManejoErrores.CreateErrorResponse(
                TipoError.ErrorValidacion,
                "Errores de validación en los datos proporcionados",
                JsonSerializer.Serialize(errors));

            // CONFIGURACIÓN DE RESPUESTA HTTP:
            // Establece código 400 (Bad Request) apropiado para errores de validación
            // Devuelve la respuesta JSON estructurada con detalles de errores
            context.Result = new ObjectResult(respuestaError)
            {
                StatusCode = 400
            };

            // MARCAR EXCEPCIÓN COMO MANEJADA:
            // Evita que otros filtros procesen esta excepción de validación
            context.ExceptionHandled = true;
        }
    }
}