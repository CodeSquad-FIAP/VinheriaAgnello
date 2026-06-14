using Microsoft.Extensions.DependencyInjection;
using VinheriaAgnello.Domain.Interfaces;

namespace VinheriaAgnello.Services;

/// <summary>
/// Métodos de extensão para registrar os serviços de aplicação no container de DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEstoqueService, EstoqueService>();
        return services;
    }
}
