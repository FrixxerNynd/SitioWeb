# Middleware - Middleware Personalizado

El middleware procesa peticiones HTTP antes de llegar a los controladores. Similar al middleware en Express.js.

## 📋 Responsabilidades

- Interceptar peticiones HTTP
- Procesar headers, autenticación, logging
- Manejar errores globalmente
- Implementar funcionalidades transversales

## 🔗 Conexiones

- **Se ejecuta**: Entre la petición HTTP y el Controller
- **Accede a**: HttpContext, Request, Response
- **Puede llamar**: Servicios inyectados
- **Se configura en**: Program.cs (pipeline)

## 🛠️ Tipos de Middleware

### Autenticación y Autorización
- `JwtMiddleware.cs` - Validación de tokens JWT
- `RoleMiddleware.cs` - Verificación de roles

### Logging y Monitoreo
- `RequestLoggingMiddleware.cs` - Log de peticiones
- `PerformanceMiddleware.cs` - Medición de rendimiento

### Manejo de Errores
- `ExceptionMiddleware.cs` - Manejo global de errores
- `ValidationMiddleware.cs` - Validación automática

### Utilidades
- `CorsMiddleware.cs` - Configuración CORS personalizada
- `RateLimitingMiddleware.cs` - Límite de peticiones

## 💡 Ejemplo de Estructura

```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

## ⚙️ Configuración

- Registro en Program.cs
- Orden de ejecución importante
- Inyección de dependencias disponible
- Configuración del pipeline de ASP.NET Core