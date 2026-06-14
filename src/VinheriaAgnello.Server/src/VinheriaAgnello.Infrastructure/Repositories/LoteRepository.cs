using Microsoft.EntityFrameworkCore;
using VinheriaAgnello.Domain.Entities;
using VinheriaAgnello.Domain.Interfaces;

namespace VinheriaAgnello.Infrastructure.Repositories;

/// <summary>
/// Repositório específico para <see cref="Lote"/>.
/// Implementa as consultas de saldo e lotes ativos por vinho.
/// </summary>
public class LoteRepository : BaseRepository<Lote>, ILoteRepository
{
    public LoteRepository(DbContext context) : base(context) { }

    public async Task<IReadOnlyList<Lote>> GetLotesAtivosPorVinhoAsync(int vinhoId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
                      .Include(l => l.Vinho)
                      .Include(l => l.Fornecedor)
                      .Where(l => l.VinhoId == vinhoId && l.Ativo && l.Quantidade > 0)
                      .ToListAsync(cancellationToken);

    public async Task<int> GetSaldoVinhoAsync(int vinhoId, CancellationToken cancellationToken = default)
        => await DbSet.Where(l => l.VinhoId == vinhoId && l.Ativo)
                      .SumAsync(l => l.Quantidade, cancellationToken);
}
