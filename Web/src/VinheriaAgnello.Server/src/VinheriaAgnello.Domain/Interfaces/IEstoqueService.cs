using VinheriaAgnello.Domain.Entities;

namespace VinheriaAgnello.Domain.Interfaces;

/// <summary>
/// Contrato do serviço de estoque da Vinheria Agnello.
/// <para>Define as operações de negócio relacionadas ao controle de estoque:
/// consultas, movimentações e relatórios.</para>
/// </summary>
public interface IEstoqueService
{
    // --- Consultas ---

    /// <summary>Obtém todos os vinhos ativos com saldo disponível.</summary>
    Task<IReadOnlyList<Vinho>> GetVinhosEmEstoqueAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtém o saldo total em garrafas de um vinho específico.</summary>
    Task<int> GetSaldoVinhoAsync(int vinhoId, CancellationToken cancellationToken = default);

    /// <summary>Lista os lotes de um vinho com saldo disponível.</summary>
    Task<IReadOnlyList<Lote>> GetLotesDoVinhoAsync(int vinhoId, CancellationToken cancellationToken = default);

    /// <summary>Obtém o histórico de movimentações de um vinho.</summary>
    Task<IReadOnlyList<TransacaoEstoque>> GetHistoricoVinhoAsync(int vinhoId, CancellationToken cancellationToken = default);

    // --- Movimentações ---

    /// <summary>Registra entrada de mercadoria no estoque.</summary>
    Task RegistrarEntradaAsync(int vinhoId, int loteId, int quantidade, decimal valorUnitario,
                               string? descricao = null, int? usuarioId = null,
                               CancellationToken cancellationToken = default);

    /// <summary>Registra saída de mercadoria do estoque (venda, perda, ajuste).</summary>
    Task RegistrarSaidaAsync(int vinhoId, int quantidade, decimal valorUnitario,
                              string? descricao = null, int? usuarioId = null,
                              CancellationToken cancellationToken = default);

    // --- Relatórios ---

    /// <summary>Obtém valor total do estoque (custo × saldo).</summary>
    Task<decimal> GetValorTotalEstoqueAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtém quantidade total de garrafas em estoque.</summary>
    Task<int> GetQuantidadeTotalGarrafasAsync(CancellationToken cancellationToken = default);
}
