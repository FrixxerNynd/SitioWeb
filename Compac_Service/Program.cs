using System.Text;
using CRM.Config;
using Serilog;
using back_cabs.CRM.contexts;
using back_cabs.CRM.services;
using back_cabs.CRM.services.Auth;
using back_cabs.services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using back_cabs.CRM.Middleware;
using StackExchange.Redis;
using back_cabs.CRM.Interfaces;
using back_cabs.CRM.services.shared;

using Microsoft.Data.SqlClient;

using back_cabs.CRM.middleware;

//RecaptchaService
using login_service.services;

var builder = WebApplication.CreateBuilder(args);

// Redis cache registration
// Configurar ConnectionMultiplexer como Singleton para compartir la conexión
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("RedisConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        // Default a localhost con timeouts cortos para desarrollo
        connectionString = "localhost:6379,abortConnect=false,connectTimeout=500,syncTimeout=500,connectRetry=1,responseTimeout=500";
    }

    var configuration = ConfigurationOptions.Parse(connectionString, true);
    configuration.AbortOnConnectFail = false; // Importante: No fallar el startup si Redis no está

    return ConnectionMultiplexer.Connect(configuration);
});

// Configurar IDistributedCache para usar el Mismo multiplexer
// Usamos Configure con IServiceProvider para evitar BuildServiceProvider() y el warning ASP0000
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.InstanceName = "CABS_pruebas";
});

builder.Services.AddOptions<Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions>()
    .Configure<IServiceProvider>((options, sp) =>
    {
        options.ConnectionMultiplexerFactory = () =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return Task.FromResult(multiplexer);
        };
    });

// Registrar el servicio de reCAPTCHA con HttpClient
builder.Services.AddHttpClient<RecaptchaService>();

// registrar CacheService
builder.Services.AddScoped<ICacheService, CacheService>();

// Configurar Serilog temprano para capturar logs de startup
builder.Host.UseSerilog();

// ✅ Registrar IHttpContextAccessor para acceder al usuario autenticado en repositorios
builder.Services.AddHttpContextAccessor();

// Agregar configuraciones centralizadas
builder.Services.AddLoggingConfiguration(builder.Configuration);
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddValidationConfiguration();
builder.Services.AddMediatRConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// ✅ MEJORA 5: Configurar Anti-Forgery (CSRF Protection)
builder.Services.AddAntiforgery(options =>
{
    // Nombre del header donde el cliente enviará el token
    options.HeaderName = "X-XSRF-TOKEN";

    // Nombre de la cookie que almacenará el token
    options.Cookie.Name = "XSRF-TOKEN";

    // CRÍTICO: HttpOnly=false para que JavaScript pueda leer el token
    // Esto es seguro porque el token no es sensible por sí mismo
    options.Cookie.HttpOnly = false;

    // Secure=None para desarrollo (permitir HTTP), cambiar a Always en producción
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;

    // SameSite=Lax para desarrollo, cambiar a Strict en producción
    options.Cookie.SameSite = SameSiteMode.Lax;

    // Path raíz para disponibilidad en toda la app
    options.Cookie.Path = "/";
});

// Servicios básicos de ASP.NET Core con configuración JSON en camelCase
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializar propiedades en camelCase para compatibilidad con frontend
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

        // Permitir lectura de números como strings
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;

        // Configurar timezone para fechas
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Inyección de contextos de base de datos
builder.Services.AddDbContext<ReadOnlyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<WriteContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(30);
            // ✅ EnableRetryOnFailure NO HABILITADO 
            // Las estrategias de reintento no son compatibles con BeginTransactionAsync()
        }
    ));

// ═══════════════════════════════════════════════════════════════
// CONTEXTO LEGACY COMPAC - Base de datos adCABS2016
// ═══════════════════════════════════════════════════════════════
builder.Services.AddDbContext<LegacyCompacReadOnlyContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CompacConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(30); // 30 segundos para consultas complejas
            // EnableRetryOnFailure removido para permitir transacciones manuales
        }
    )
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)); // Solo lectura

// Contexto de escritura (usar con precaución - datos legacy)
// IMPORTANTE: EnableRetryOnFailure está DESHABILITADO para permitir transacciones manuales
// Si se necesita retry logic, usar Database.CreateExecutionStrategy()
builder.Services.AddDbContext<LegacyCompacWriteContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CompacConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(30); // 30 segundos para operaciones de escritura
            // ✅ EnableRetryOnFailure NO HABILITADO
            // Las estrategias de reintento no son compatibles con BeginTransactionAsync()
        }
    )); // Change Tracking habilitado por defecto

// ═══════════════════════════════════════════════════════════════
// UNIT OF WORK PATTERN
// ═══════════════════════════════════════════════════════════════
// Coordina transacciones entre múltiples repositorios
// Garantiza atomicidad (todo o nada) en operaciones complejas
builder.Services.AddScoped<back_cabs.CRM.Core.UnitOfWork.IUnitOfWork, back_cabs.CRM.Core.UnitOfWork.UnitOfWork>();

// ═══════════════════════════════════════════════════════════════
// REPOSITORIOS (REPOSITORY PATTERN)
// ═══════════════════════════════════════════════════════════════
// Nota: Los repositorios se mantienen para uso directo cuando no se requiere transacción
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Auth.IUsuarioAuthRepository, back_cabs.CRM.repositories.Auth.UsuarioAuthRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAgenteRepository, back_cabs.CRM.repositories.Legacy.AdmAgenteRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMonedaRepository, back_cabs.CRM.repositories.Legacy.AdmMonedaRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAlmacenRepository, back_cabs.CRM.repositories.Legacy.AdmAlmacenRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmProductoRepository, back_cabs.CRM.repositories.Legacy.AdmProductoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmDocumentoModeloRepository, back_cabs.CRM.repositories.Legacy.AdmDocumentoModeloRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmConceptoRepository, back_cabs.CRM.repositories.Legacy.AdmConceptoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmNumeroSerieRepository, back_cabs.CRM.repositories.Legacy.AdmNumeroSerieRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmDocumentoRepository, back_cabs.CRM.repositories.Legacy.AdmDocumentoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmClienteRepository, back_cabs.CRM.repositories.Legacy.AdmClienteRepository>();

// Inyección de servicios de la aplicación
builder.Services.AddScoped<IServicioJwt, ServicioJwt>(); // ✅ Ahora usa interfaz para mejor testabilidad
builder.Services.AddScoped<UsuarioAuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<HmacOtpService>(); // OTP stateless (sin BD)

// ═══════════════════════════════════════════════════════════════
// SERVICIOS DE GENERACIÓN DE PDFs
// ═══════════════════════════════════════════════════════════════
// Servicio de envío de PDFs por correo (actualizado para usar PdfMakeService)
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Shared.IPdfEmailService, back_cabs.CRM.services.shared.PdfEmailService>();

// Registro de servicios Legacy (solo conexión directa a BD legacy adCABS2016)
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAgenteService, back_cabs.CRM.services.Legacy.AdmAgenteService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMonedaService, back_cabs.CRM.services.Legacy.AdmMonedaService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAlmacenService, back_cabs.CRM.services.Legacy.AdmAlmacenService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmProductoService, back_cabs.CRM.services.Legacy.AdmProductoService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmDocumentoModeloService, back_cabs.CRM.services.Legacy.AdmDocumentoModeloService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmConceptoService, back_cabs.CRM.services.Legacy.AdmConceptoService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmNumeroSerieService, back_cabs.CRM.services.Legacy.AdmNumeroSerieService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmDocumentoService, back_cabs.CRM.services.Legacy.AdmDocumentoService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmClienteService, back_cabs.CRM.services.Legacy.AdmClienteService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmUnidadMedidaPesoRepository, back_cabs.CRM.repositories.Legacy.AdmUnidadMedidaPesoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmUnidadMedidaPesoService, back_cabs.CRM.services.Legacy.AdmUnidadMedidaPesoService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMovimientoSerieRepository, back_cabs.CRM.repositories.Legacy.AdmMovimientoSerieRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMovimientoSerieService, back_cabs.CRM.services.Legacy.AdmMovimientoSerieService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAgenteLegacyEnlaceService, back_cabs.CRM.services.Legacy.AgenteLegacyEnlaceService>();


// Servicios de procesamiento de imágenes y gestión de archivos
builder.Services.AddScoped<back_cabs.CRM.services.shared.ImageProcessingService>();


// Servicios Legacy - Catálogos Adminpaq
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMonedaRepository, back_cabs.CRM.repositories.Legacy.AdmMonedaRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAgenteRepository, back_cabs.CRM.repositories.Legacy.AdmAgenteRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMonedaService, back_cabs.CRM.services.Legacy.AdmMonedaService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAgenteService, back_cabs.CRM.services.Legacy.AdmAgenteService>();

// Registrar la conexión a la base de datos para inyectar IDbConnection
builder.Services.AddTransient<System.Data.IDbConnection>(sp =>
    new Microsoft.Data.SqlClient.SqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Validación de tokens JWT en rutas específicas
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("user", "admin"));
});

// Configuración de Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("API Status", () => HealthCheckResult.Healthy("API is up and running"))
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
        name: "Database")
    .AddCheck("Custom Health Check", () =>
    {
        // Lógica de verificación personalizada
        bool healthCheckPassed = true; // Reemplazar con lógica real
        return healthCheckPassed ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    });

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureFrontend", policy =>
    {
        policy.WithOrigins(
            "http://192.168.10.5:4200",  // ✅ Correcto (Angular en desarrollo/ng serve)
            "http://192.168.10.5",       // ✅ Correcto (Si en IIS usas el puerto 80 por defecto)
            "http://192.168.10.5:8081",  // ✅ Correcto (Si en IIS configuraste un puerto específico)
            "http://localhost:4200",
            "http://localhost:5176") // Angular
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // CRÍTICO: Para cookies HttpOnly
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .WithExposedHeaders("X-CSRF-Token"); // Para CSRF protection
    });

    // Política más restrictiva para producción
    options.AddPolicy("Production", policy =>
{
    policy.WithOrigins(
            "http://192.168.10.5:4200",  // ✅ Correcto (Angular en desarrollo/ng serve)
            "http://192.168.10.5",       // ✅ Correcto (Si en IIS usas el puerto 80 por defecto)
            "http://192.168.10.5:8081",  // ✅ Correcto (Si en IIS configuraste un puerto específico)
            "http://localhost:4200",
            "http://localhost:5176"
        )
        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
        .AllowAnyHeader()
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromHours(24))
        .WithExposedHeaders("X-CSRF-Token");
});
    options.AddPolicy("Prueba", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Permite CUALQUIER origen dinámicamente
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials()
      .WithExposedHeaders("X-CSRF-Token"); // Para CSRF protection
    });
});
// Configuración de cookies seguras
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AuthToken";
    options.Cookie.HttpOnly = true; // CRÍTICO: Previene acceso desde JavaScript
    options.Cookie.SecurePolicy = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest; // Solo HTTPS en producción
    options.Cookie.SameSite = builder.Environment.IsProduction()
        ? SameSiteMode.Strict
        : SameSiteMode.Lax; // CSRF protection
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// Configuración de límite de tamaño de archivos para subida
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

var app = builder.Build();



// Middleware pipeline con seguridad mejorada
app.UseGlobalErrorHandling();
app.UseSecurityHeaders();
app.UseRequestResponseLogging();

// CORS (usar política apropiada según el entorno)
// var corsPolicy = app.Environment.IsProduction() ? "Production" : "SecureFrontend";
// app.UseCors(corsPolicy);
app.UseCors("SecureFrontend"); // Usar política de desarrollo para pruebas, cambiar a "Production" en producción

// Servir archivos estáticos (para Swagger UI custom scripts)
app.UseStaticFiles();

// Routing
app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Swagger UI (solo en desarrollo y staging)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {

        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API v1");
        c.RoutePrefix = "swagger";
        // Inyectar script para manejar CSRF token automáticamente
        c.InjectJavascript("/swagger-ui/csrf-interceptor.js");

    });
}

// ✅ MEJORA 5: Activar validación CSRF (debe ir DESPUÉS de auth)
app.UseCsrfValidation();

// Health checks
app.UseHealthChecksConfiguration();

// Controladores
app.MapControllers();

// Logging de inicio
app.Logger.LogInformation("🚀 CRM API iniciada correctamente");

try
{
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Error fatal en la aplicación");
    throw;
}

// Rate limiting personalizado para endpoints críticos
app.UseWhen(context =>
    context.Request.Path.StartsWithSegments("/api/auth/registro") ||
    context.Request.Path.StartsWithSegments("/api/auth/login") ||
    context.Request.Path.StartsWithSegments("/api/files/upload") ||
    context.Request.Path.StartsWithSegments("/api/reparaciones") && context.Request.Method == "POST" ||
    context.Request.Path.StartsWithSegments("/api/fotosevaluacion") && context.Request.Method == "POST",
    appBuilder =>
    {
        appBuilder.UseMiddleware<CustomRateLimitMiddleware>();
    });

// Rate limiting global para todas las rutas (excepto static files, swagger, health)
app.UseWhen(context =>
    !context.Request.Path.StartsWithSegments("/swagger") &&
    !context.Request.Path.StartsWithSegments("/health") &&
    !context.Request.Path.StartsWithSegments("/static"),
    appBuilder =>
    {
        appBuilder.UseMiddleware<CustomRateLimitMiddleware>();
    });
