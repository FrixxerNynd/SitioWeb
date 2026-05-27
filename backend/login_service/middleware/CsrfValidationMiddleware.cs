// =====================================================================================
// CSRF VALIDATION MIDDLEWARE - CsrfValidationMiddleware.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE MIDDLEWARE?
// Valida tokens Anti-Forgery (CSRF) en requests POST, PUT, DELETE y PATCH
// para prevenir ataques Cross-Site Request Forgery.
//
// CUÁNDO SE EJECUTA:
// - En todas las requests que modifican datos (POST, PUT, DELETE, PATCH)
// - Excluye /api/auth/login (primera request sin token)
// - Excluye /api/auth/csrf-token (endpoint para obtener token)
//
// CÓMO FUNCIONA:
// 1. Cliente solicita token CSRF: GET /api/auth/csrf-token
// 2. Server genera token y lo envía en cookie + respuesta JSON
// 3. Cliente incluye token en header X-XSRF-TOKEN en requests siguientes
// 4. Middleware valida que el token sea válido antes de procesar request
//
// SEGURIDAD:
// ✅ Previene CSRF cuando se usan cookies para autenticación
// ✅ Token almacenado en cookie HttpOnly=false (debe ser legible por JS)
// ✅ Token validado en cada request que modifica datos
// ✅ Excluye login para permitir primera autenticación
//
// =====================================================================================

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Hosting;

namespace back_cabs.CRM.middleware
{
    /// <summary>
    /// Middleware para validación de tokens CSRF en requests que modifican datos
    /// </summary>
    public class CsrfValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;
        private readonly ILogger<CsrfValidationMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        // Rutas que NO requieren validación CSRF
        private readonly HashSet<string> _excludedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/auth/login",           // Login inicial no tiene token
            "/api/auth/registro",        // Registro no requiere token
            "/api/auth/registro-cliente",
            "/api/auth/csrf-token",      // Endpoint para obtener token
            "/api/auth/refresh",         // Refresh token tampoco requiere validación
            "/api/GastoViaticos",        // Viáticos (temporal durante desarrollo)
            "/api/AdmDocumentos",
            "/api/vehiculos",            // Vehiculos - issue con CSRF en Windows Server cross-origin
            "/api/Vehiculos",            // Vehiculos (case-sensitive check)
            "/swagger",                  // Swagger UI (desarrollo)
            "/hubs/"                     // SignalR hubs (WebSocket connections)
        };

        public CsrfValidationMiddleware(
            RequestDelegate next,
            IAntiforgery antiforgery,
            ILogger<CsrfValidationMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Solo validar en métodos que modifican datos
            if (RequiresCsrfValidation(context.Request))
            {
                // PRIMERO: Verificar si la ruta está en la lista de exclusión (siempre se respeta)
                if (IsExcludedPath(context.Request.Path))
                {
                    _logger.LogDebug(
                        "Skipping CSRF validation for excluded path: {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);
                    
                    await _next(context);
                    return;
                }

                // SEGUNDO: En desarrollo, excluir todas las rutas /api para facilitar testing
                if (_environment.IsDevelopment() && context.Request.Path.StartsWithSegments("/api"))
                {
                    _logger.LogDebug(
                        "Skipping CSRF validation in Development for API path: {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);
                    
                    await _next(context);
                    return;
                }

                // TERCERO: Validar CSRF para rutas no excluidas
                try
                {
                    // Log detallado antes de validar
                    var csrfHeaderValue = context.Request.Headers["X-XSRF-TOKEN"].FirstOrDefault();
                    var xsrfCookiePresent = context.Request.Cookies.ContainsKey("XSRF-TOKEN");
                    var xsrfCookieValue = context.Request.Cookies.TryGetValue("XSRF-TOKEN", out var cookieVal) ? cookieVal?.Substring(0, 20) : "N/A";
                    
                    _logger.LogWarning(
                        "CSRF Validation DEBUG for {Method} {Path}: Header={HeaderValue}, Cookie={CookiePresent} ({CookieValue}), Origin={Origin}",
                        context.Request.Method,
                        context.Request.Path,
                        csrfHeaderValue?.Substring(0, 20) ?? "(missing)",
                        xsrfCookiePresent,
                        xsrfCookieValue,
                        context.Request.Headers["Origin"].FirstOrDefault());
                    
                    // Validar el token CSRF
                    await _antiforgery.ValidateRequestAsync(context);
                    
                    _logger.LogDebug(
                        "CSRF token validated successfully for {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);
                }
                catch (AntiforgeryValidationException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "CSRF validation failed for {Method} {Path} from {IpAddress}. Headers: X-XSRF-TOKEN={CsrfHeader}, Cookie={CookiePresent}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Connection.RemoteIpAddress,
                        context.Request.Headers["X-XSRF-TOKEN"].FirstOrDefault() ?? "(missing)",
                        context.Request.Cookies.ContainsKey("XSRF-TOKEN") ? "present" : "missing");

                    // Retornar 403 Forbidden con mensaje explicativo
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "CSRF Validation Failed",
                        message = "Invalid or missing anti-forgery token. Please refresh the page and try again.",
                        code = "CSRF_TOKEN_INVALID",
                        statusCode = 403
                    });

                    return; // No continuar con el pipeline
                }
            }

            // Continuar con el siguiente middleware
            await _next(context);
        }

        /// <summary>
        /// Determina si el request requiere validación CSRF
        /// </summary>
        private bool RequiresCsrfValidation(HttpRequest request)
        {
            // Solo validar métodos que modifican datos
            return HttpMethods.IsPost(request.Method) ||
                   HttpMethods.IsPut(request.Method) ||
                   HttpMethods.IsDelete(request.Method) ||
                   HttpMethods.IsPatch(request.Method);
        }

        /// <summary>
        /// Verifica si la ruta está excluida de validación CSRF
        /// </summary>
        private bool IsExcludedPath(PathString path)
        {
            // Verificar rutas que comienzan con el patrón excluido
            foreach (var excluded in _excludedPaths)
            {
                if (path.Value?.StartsWith(excluded, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Extension method para registrar el middleware de CSRF en el pipeline
    /// </summary>
    public static class CsrfValidationMiddlewareExtensions
    {
        /// <summary>
        /// Agrega el middleware de validación CSRF al pipeline de la aplicación
        /// </summary>
        /// <param name="builder">IApplicationBuilder</param>
        /// <returns>IApplicationBuilder para encadenamiento</returns>
        public static IApplicationBuilder UseCsrfValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CsrfValidationMiddleware>();
        }
    }
}
