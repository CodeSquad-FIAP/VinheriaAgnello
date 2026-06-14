using Microsoft.EntityFrameworkCore;
using VinheriaAgnello.Domain.Entities;
using VinheriaAgnello.Domain.Interfaces;

namespace VinheriaAgnello.Infrastructure.Repositories;

/// <summary>
/// Repositório específico para <see cref="TransacaoEstoque"/>.
/// Implementa consultas de histórico por vinho e período.
/// </summary>
public class TransacaoEstoqueRepository : BaseRepository<TransacaoEstoque>, ITransacaoEstoqueRepository
{
    public TransacaoEstoqueRepository(DbContext context) : base(context) { }

    public async Task<IReadOnlyList<TransacaoEstoque>> GetPorVinhoAsync(int vinhoId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
                      .Include(t => t.Vinho)
                      .Include(t => t.Lote)
                      .Where(t => t.VinhoId == vinhoId)
                      .OrderByDescending(t => t.DataMovimentacao)
                      .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TransacaoEstoque>> GetPorPeriodoAsync(
        DateTime inicio, DateTime fim, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
                      .Include(t => t.Vinho)
                      .Where(t => t.DataMovimentacao >= inicio && t.DataMovimentacao <= fim)
                      .OrderByDescending(t => t.DataMovimentacao)
                      .ToListAsync(cancellationToken);
}
