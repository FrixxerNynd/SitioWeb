// =================================================================================================
// TIPOS DE ERROR - TipoError.cs
// =================================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Este archivo define un sistema de tipos de error centralizado para toda la aplicación ASP.NET Core.
// Proporciona una enumeración de errores comunes con códigos HTTP asociados, descripciones
// localizadas en español, y métodos de extensión para conversión automática.
//
// FUNCIONALIDADES PRINCIPALES:
// - Enumeración TipoError con todos los tipos de error de la aplicación
// - Método de extensión ToHttpStatusCode() para conversión automática a códigos HTTP
// - Método GetDescription() para obtener descripciones localizadas en español
// - Categorización de errores por dominio (validación, autorización, servidor, etc.)
// - Soporte para errores personalizados específicos de negocio
//
// ¿CÓMO SE USA?
// 1. EN CONTROLADORES (para respuestas de error):
//    if (!ModelState.IsValid)
//        return BadRequest(TipoError.ErrorValidacion.GetDescription());
//
// 2. EN SERVICIOS (para lanzar excepciones tipificadas):
//    if (usuario == null)
//        throw new NotFoundException(TipoError.ErrorNoEncontrado.GetDescription());
//
// 3. EN MIDDLEWARE (para categorizar errores automáticamente):
//    var codigoHttp = tipoError.ToHttpStatusCode();
//    var descripcion = tipoError.GetDescription();
//
// 4. EN VALIDADORES FLUENT (para errores de validación):
//    RuleFor(x => x.Email).NotEmpty().WithMessage(TipoError.ErrorValidacion.GetDescription());
//
// ¿EN QUÉ CASOS SE USA?
// - ERRORES DE VALIDACIÓN (400): Datos inválidos, campos requeridos faltantes, formatos incorrectos
// - ERRORES DE AUTENTICACIÓN (401): Credenciales faltantes, tokens inválidos o expirados
// - ERRORES DE AUTORIZACIÓN (403): Usuario autenticado pero sin permisos para la operación
// - ERRORES DE RECURSOS (404): Endpoints no existentes, entidades no encontradas en BD
// - ERRORES DE CONFLICTO (409): Operaciones que violan restricciones de negocio o estado
// - ERRORES DE ENTIDAD (422): Datos sintácticamente correctos pero semánticamente inválidos
// - ERRORES DE LÍMITE (429): Rate limiting excedido, demasiadas requests por minuto
// - ERRORES DEL SERVIDOR (500): Excepciones no manejadas, errores internos inesperados
// - ERRORES DE SERVICIO (503): Servicios externos caídos, timeouts en integraciones
// - ERRORES DE BASE DE DATOS (520): Conexiones fallidas, deadlocks, constraint violations
// - ERRORES EXTERNOS (521): Fallos en APIs de terceros, servicios de pago caídos
// - ERRORES DE CONFIGURACIÓN (522): Variables de entorno faltantes, configuración inválida
//
// EJEMPLOS PRÁCTICOS:
//
// 1. VALIDACIÓN EN CONTROLADOR:
//    [HttpPost("usuarios")]
//    public IActionResult CrearUsuario([FromBody] CrearUsuarioRequest request)
//    {
//        if (!ModelState.IsValid)
//            return BadRequest(new {
//                exito = false,
//                mensaje = TipoError.ErrorValidacion.GetDescription(),
//                errores = ModelState.Values.SelectMany(v => v.Errors)
//            });
//    }
//
// 2. AUTORIZACIÓN EN SERVICIO:
//    public async Task<Usuario> ObtenerUsuarioPorId(int id)
//    {
//        var usuario = await _repositorio.ObtenerPorId(id);
//        if (usuario == null)
//            throw new NotFoundException(TipoError.ErrorNoEncontrado.GetDescription());
//        return usuario;
//    }
//
// 3. MANEJO DE ERRORES EN MIDDLEWARE:
//    switch (excepcion)
//    {
//        case ValidationException:
//            return TipoError.ErrorValidacion;
//        case UnauthorizedAccessException:
//            return TipoError.ErrorNoAutorizado;
//        default:
//            return TipoError.ErrorServidorInterno;
//    }
//
// INTEGRACIÓN CON OTROS COMPONENTES:
// - Funciona con MiddlewareManejoErrores para respuestas HTTP consistentes
// - Compatible con FiltrosExcepcionesApi para manejo granular de excepciones
// - Se integra con validadores FluentValidation para mensajes de error
// - Compatible con logging estructurado de Serilog
//
// VENTAJAS DE USAR ESTE SISTEMA:
// - CONSISTENCIA: Todos los errores siguen el mismo formato y códigos HTTP
// - LOCALIZACIÓN: Mensajes en español adaptados al contexto de negocio
// - MANTENIBILIDAD: Cambiar un mensaje de error en un solo lugar
// - DEBUGGING: Fácil identificación del tipo de error por código HTTP
// - API DESIGN: Respuestas RESTful consistentes según estándares
//
// DEPENDENCIAS EXTERNAS:
// - System.Net (para HttpStatusCode)
// - Microsoft.AspNetCore.Mvc (para integración con controladores)
//
// NOTAS DE IMPLEMENTACIÓN:
// - Los códigos HTTP siguen estándares RFC 7231 y RFC 7235
// - Las descripciones están optimizadas para UX (claridad y acción correctiva)
// - Thread-safe y puede usarse en contextos concurrentes
// - No contiene lógica de negocio, solo definiciones de tipos de error

namespace back_cabs.CRM.Middleware;

/// <summary>
/// Enumeración de tipos de error con sus códigos HTTP asociados.
/// Define todos los tipos de error manejados por la aplicación.
/// Cada valor representa un código HTTP estándar y su significado semántico.
/// </summary>
public enum TipoError
{
    // ==========================================
    // ERRORES DE CLIENTE (4xx)
    // ==========================================

    /// <summary>
    /// Error de validación de datos de entrada (400 Bad Request).
    /// Se usa cuando los datos proporcionados por el cliente no cumplen con los requisitos básicos
    /// de formato, tipo o presencia obligatoria.
    /// </summary>
    ErrorValidacion = 400,

    /// <summary>
    /// Error de autenticación faltante o inválida (401 Unauthorized).
    /// Se usa cuando el cliente no proporciona credenciales válidas o cuando el token JWT
    /// ha expirado o es inválido.
    /// </summary>
    ErrorNoAutorizado = 401,

    /// <summary>
    /// Error de permisos insuficientes para la operación (403 Forbidden).
    /// Se usa cuando el usuario está autenticado pero no tiene los permisos necesarios
    /// para realizar la acción solicitada.
    /// </summary>
    ErrorProhibido = 403,

    /// <summary>
    /// Error de recurso no encontrado (404 Not Found).
    /// Se usa cuando el endpoint solicitado no existe o cuando se busca una entidad
    /// que no existe en la base de datos.
    /// </summary>
    ErrorNoEncontrado = 404,

    /// <summary>
    /// Error de conflicto con el estado actual del recurso (409 Conflict).
    /// Se usa cuando la operación solicitada no puede completarse debido a restricciones
    /// de negocio o estado actual incompatible.
    /// </summary>
    ErrorConflicto = 409,

    /// <summary>
    /// Error de entidad semánticamente inválida (422 Unprocessable Entity).
    /// Se usa cuando los datos están bien formados pero no pueden procesarse debido
    /// a reglas de negocio o lógica de aplicación.
    /// </summary>
    ErrorEntidadNoProcesable = 422,

    /// <summary>
    /// Error de límite de solicitudes excedido (429 Too Many Requests).
    /// Se usa cuando el cliente ha excedido el límite de requests permitido
    /// en un período de tiempo (rate limiting).
    /// </summary>
    ErrorDemasiadasSolicitudes = 429,

    // ==========================================
    // ERRORES DEL SERVIDOR (5xx)
    // ==========================================

    /// <summary>
    /// Error interno del servidor no esperado (500 Internal Server Error).
    /// Se usa para excepciones no manejadas, errores de lógica o problemas
    /// internos que requieren atención del equipo de desarrollo.
    /// </summary>
    ErrorServidorInterno = 500,

    /// <summary>
    /// Error de servicio no disponible temporalmente (503 Service Unavailable).
    /// Se usa cuando servicios externos o internos están temporalmente fuera de servicio
    /// debido a mantenimiento, sobrecarga o fallos temporales.
    /// </summary>
    ErrorServicioNoDisponible = 503,

    // ==========================================
    // ERRORES PERSONALIZADOS DE APLICACIÓN (52x)
    // ==========================================

    /// <summary>
    /// Error de conexión o consulta a base de datos (520 Database Error).
    /// Se usa para problemas específicos de base de datos como timeouts de conexión,
    /// constraint violations, deadlocks o problemas de conectividad.
    /// </summary>
    ErrorBaseDatos = 520,

    /// <summary>
    /// Error en comunicación con servicios externos (521 External Service Error).
    /// Se usa cuando fallan las integraciones con APIs de terceros, servicios de pago,
    /// servicios de email, o cualquier dependencia externa.
    /// </summary>
    ErrorServicioExterno = 521,

    /// <summary>
    /// Error de configuración del sistema o variables faltantes (522 Configuration Error).
    /// Se usa cuando hay problemas de configuración como variables de entorno faltantes,
    /// archivos de configuración corruptos o parámetros inválidos.
    /// </summary>
    ErrorConfiguracion = 522
}

/// <summary>
/// Clase de extensiones para el enum TipoError.
/// Proporciona métodos utilitarios para trabajar con tipos de error.
/// Permite conversión automática a códigos HTTP y obtención de descripciones localizadas.
/// </summary>
public static class ExtensionesTipoError
{
    /// <summary>
    /// Convierte un TipoError a su código HTTP correspondiente.
    /// Método de extensión que facilita el uso en controladores y middlewares.
    /// </summary>
    /// <param name="tipoError">El tipo de error a convertir</param>
    /// <returns>Código HTTP como entero (ej: 400, 404, 500)</returns>
    /// <example>
    /// var statusCode = TipoError.ErrorValidacion.ToHttpStatusCode(); // Retorna 400
    /// </example>
    public static int ToHttpStatusCode(this TipoError tipoError)
    {
        // CONVERSIÓN DIRECTA: El valor del enum ya representa el código HTTP
        // Esto permite mantener consistencia entre definición y uso
        return (int)tipoError;
    }

    /// <summary>
    /// Obtiene la descripción localizada en español del tipo de error.
    /// Proporciona mensajes de error user-friendly optimizados para UX.
    /// </summary>
    /// <param name="tipoError">El tipo de error del cual obtener la descripción</param>
    /// <returns>Descripción del error en español, clara y actionable</returns>
    /// <example>
    /// var mensaje = TipoError.ErrorValidacion.GetDescription();
    /// // Retorna: "Los datos proporcionados no son válidos"
    /// </example>
    public static string GetDescription(this TipoError tipoError)
    {
        // SWITCH EXPRESSION: Mapeo directo de cada tipo de error a su descripción
        // Las descripciones están optimizadas para claridad y acción correctiva
        return tipoError switch
        {
            // ERRORES DE VALIDACIÓN (400)
            TipoError.ErrorValidacion => "Los datos proporcionados no son válidos",

            // ERRORES DE AUTENTICACIÓN/AUTORIZACIÓN (401/403)
            TipoError.ErrorNoAutorizado => "No autorizado para acceder a este recurso",
            TipoError.ErrorProhibido => "Acceso prohibido a este recurso",

            // ERRORES DE RECURSOS (404)
            TipoError.ErrorNoEncontrado => "El recurso solicitado no fue encontrado",

            // ERRORES DE CONFLICTO (409)
            TipoError.ErrorConflicto => "Conflicto con el estado actual del recurso",

            // ERRORES DE ENTIDAD (422)
            TipoError.ErrorEntidadNoProcesable => "Los datos no pueden ser procesados",

            // ERRORES DE LÍMITE (429)
            TipoError.ErrorDemasiadasSolicitudes => "Demasiadas solicitudes, intente más tarde",

            // ERRORES DEL SERVIDOR (5xx)
            TipoError.ErrorServidorInterno => "Error interno del servidor",
            TipoError.ErrorServicioNoDisponible => "Servicio no disponible temporalmente",

            // ERRORES PERSONALIZADOS (52x)
            TipoError.ErrorBaseDatos => "Error de conexión con la base de datos",
            TipoError.ErrorServicioExterno => "Error en servicio externo",
            TipoError.ErrorConfiguracion => "Error de configuración del sistema",

            // CASO POR DEFECTO: Error desconocido (no debería ocurrir en código bien diseñado)
            _ => "Error desconocido"
        };
    }
}