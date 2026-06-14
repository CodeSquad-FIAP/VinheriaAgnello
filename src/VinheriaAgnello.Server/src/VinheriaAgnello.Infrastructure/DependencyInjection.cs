using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VinheriaAgnello.Domain.Entities;
using VinheriaAgnello.Domain.Interfaces;
using VinheriaAgnello.Infrastructure.Data;
using VinheriaAgnello.Infrastructure.Repositories;

namespace VinheriaAgnello.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar os serviços de infraestrutura
/// no container de DI da aplicação.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra o AppDbContext (SQLite), os repositórios e o UnitOfWork.
    /// </summary>
    /// <param name="services">Coleção de serviços do IServiceCollection.</param>
    /// <param name="connectionString">String de conexão SQLite (ex: "Data Source=vinheria.db").</param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IVinhoRepository, VinhoRepository>();
        services.AddScoped<ILoteRepository, LoteRepository>();
        services.AddScoped<ITransacaoEstoqueRepository, TransacaoEstoqueRepository>();

        services.AddScoped<IRepository<Fornecedor>, BaseRepository<Fornecedor>>();
        services.AddScoped<IRepository<Categoria>, BaseRepository<Categoria>>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
