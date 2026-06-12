using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CRM.Config;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Los tokens de GenerateTokens usan claims cortos ("id", "role", etc.)
                // y no incluyen iss/aud, por lo que se omite su validación.
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            // Habilitar logging detallado para debugging
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogError("JWT Authentication failed: {Exception}", context.Exception?.Message);
                    if (context.Exception != null)
                    {
                        logger.LogError("JWT Exception details: {Details}", context.Exception.ToString());
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogInformation("JWT Token validated successfully for user: {User}", 
                        context.Principal?.Identity?.Name ?? "Unknown");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    
                    // Primero intentar leer del header Authorization (para compatibilidad)
                    var token = context.Token;
                    
                    // Si no hay token en header, buscar en cookie HttpOnly
                    if (string.IsNullOrEmpty(token))
                    {
                        token = context.Request.Cookies["AuthToken"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                            logger.LogInformation("JWT Token read from HttpOnly cookie, length: {Length}", token.Length);
                        }
                    }
                    else
                    {
                        logger.LogInformation("JWT Token received from Authorization header, length: {Length}", token.Length);
                    }
                    
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        logger.LogWarning("No JWT token received in Authorization header or AuthToken cookie");
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}