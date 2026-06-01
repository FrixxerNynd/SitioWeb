using Microsoft.EntityFrameworkCore;
using back_cabs.CRM.contexts;

namespace CRM.Config;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Contexto para operaciones de solo lectura (optimizado para queries)
        services.AddDbContext<ReadOnlyContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                );
            });
            
            // Optimizaciones para solo lectura
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(false);
        });

        // Contexto para operaciones de escritura (transaccional)
        services.AddDbContext<WriteContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                );
            });
            
            // Configuraciones para escritura
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            options.EnableSensitiveDataLogging(false);
        });

        return services;
    }
}