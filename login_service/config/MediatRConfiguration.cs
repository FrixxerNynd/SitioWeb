using MediatR;

namespace CRM.Config;

public static class MediatRConfiguration
{
    public static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
    {
        // Registrar MediatR
        services.AddMediatR(typeof(MediatRConfiguration).Assembly);

        return services;
    }
}