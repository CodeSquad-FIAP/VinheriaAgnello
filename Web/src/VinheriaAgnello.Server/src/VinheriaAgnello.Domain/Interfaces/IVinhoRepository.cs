using VinheriaAgnello.Domain.Entities;

namespace VinheriaAgnello.Domain.Interfaces;

/// <summary>
/// Interface específica para repositório de <see cref="Vinho"/>.
/// </summary>
public interface IVinhoRepository : IRepository<Vinho>
{
    Task<Vinho?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vinho>> GetVinhosAtivosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vinho>> GetVinhosComEstoqueAsync(CancellationToken cancellationToken = default);
}
