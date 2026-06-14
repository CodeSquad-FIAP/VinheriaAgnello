using VinheriaAgnello.Domain.Entities;

namespace VinheriaAgnello.Domain.Interfaces;

/// <summary>
/// Interface específica para repositório de <see cref="TransacaoEstoque"/>.
/// </summary>
public interface ITransacaoEstoqueRepository : IRepository<TransacaoEstoque>
{
    Task<IReadOnlyList<TransacaoEstoque>> GetPorVinhoAsync(int vinhoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransacaoEstoque>> GetPorPeriodoAsync(DateTime inicio, DateTime fim, CancellationToken cancellationToken = default);
}
