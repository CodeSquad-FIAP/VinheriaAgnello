using VinheriaAgnello.Domain.Entities;

namespace VinheriaAgnello.Domain.Interfaces;

/// <summary>
/// Interface específica para repositório de <see cref="Lote"/>.
/// </summary>
public interface ILoteRepository : IRepository<Lote>
{
    Task<IReadOnlyList<Lote>> GetLotesAtivosPorVinhoAsync(int vinhoId, CancellationToken cancellationToken = default);
    Task<int> GetSaldoVinhoAsync(int vinhoId, CancellationToken cancellationToken = default);
}
