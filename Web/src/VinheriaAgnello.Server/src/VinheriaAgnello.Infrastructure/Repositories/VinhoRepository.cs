using Microsoft.EntityFrameworkCore;
using VinheriaAgnello.Domain.Entities;
using VinheriaAgnello.Domain.Interfaces;

namespace VinheriaAgnello.Infrastructure.Repositories;

/// <summary>
/// Repositório específico para <see cref="Vinho"/>.
/// Implementa as consultas especializadas definidas em <see cref="IVinhoRepository"/>.
/// </summary>
public class VinhoRepository : BaseRepository<Vinho>, IVinhoRepository
{
    public VinhoRepository(DbContext context) : base(context) { }

    public async Task<Vinho?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(v => v.CodigoSku == sku, cancellationToken);

    public async Task<IReadOnlyList<Vinho>> GetVinhosAtivosAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
                      .Include(v => v.Categoria)
                      .Where(v => v.Ativo)
                      .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vinho>> GetVinhosComEstoqueAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
                      .Include(v => v.Categoria)
                      .Include(v => v.Lotes)
                      .Where(v => v.Ativo && v.Lotes.Any(l => l.Ativo && l.Quantidade > 0))
                      .ToListAsync(cancellationToken);
}
