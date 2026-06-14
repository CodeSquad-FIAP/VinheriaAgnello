using VinheriaAgnello.Domain.Entities;
using VinheriaAgnello.Domain.Enums;
using VinheriaAgnello.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace VinheriaAgnello.Services;

/// <summary>
/// Implementação do serviço de estoque da Vinheria Agnello.
/// Orquestra as operações de negócio coordenando múltiplos repositórios
/// dentro de transações atômicas via Unit of Work.
/// </summary>
public class EstoqueService : IEstoqueService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<EstoqueService> _logger;

    public EstoqueService(IUnitOfWork uow, ILogger<EstoqueService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    // ========================
    //       CONSULTAS
    // ========================

    public async Task<IReadOnlyList<Vinho>> GetVinhosEmEstoqueAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando vinhos com estoque disponível...");
        return await _uow.Vinhos.GetVinhosComEstoqueAsync(cancellationToken);
    }

    public async Task<int> GetSaldoVinhoAsync(int vinhoId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando saldo do vinho ID {VinhoId}...", vinhoId);
        return await _uow.Lotes.GetSaldoVinhoAsync(vinhoId, cancellationToken);
    }

    public async Task<IReadOnlyList<Lote>> GetLotesDoVinhoAsync(int vinhoId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando lotes do vinho ID {VinhoId}...", vinhoId);
        return await _uow.Lotes.GetLotesAtivosPorVinhoAsync(vinhoId, cancellationToken);
    }

    public async Task<IReadOnlyList<TransacaoEstoque>> GetHistoricoVinhoAsync(int vinhoId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando histórico do vinho ID {VinhoId}...", vinhoId);
        return await _uow.Transacoes.GetPorVinhoAsync(vinhoId, cancellationToken);
    }

    // ========================
    //    MOVIMENTAÇÕES
    // ========================

    public async Task RegistrarEntradaAsync(
        int vinhoId, int loteId, int quantidade, decimal valorUnitario,
        string? descricao = null, int? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Registrando ENTRADA: Vinho {VinhoId}, Lote {LoteId}, Qtd {Quantidade}...",
            vinhoId, loteId, quantidade);

        await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Valida o lote
            var lote = await _uow.Lotes.GetByIdAsync(loteId, cancellationToken);
            if (lote is null)
                throw new KeyNotFoundException($"Lote ID {loteId} não encontrado.");

            // 2. Atualiza quantidade do lote
            lote.AdicionarQuantidade(quantidade);
            await _uow.Lotes.UpdateAsync(lote, cancellationToken);

            // 3. Cria transação de entrada
            var transacao = new TransacaoEstoque(
                TipoMovimentacao.Entrada,
                vinhoId,
                quantidade,
                valorUnitario,
                descricao ?? "Entrada de mercadoria",
                loteId,
                usuarioId);

            await _uow.Transacoes.AddAsync(transacao, cancellationToken);

            // 4. Persiste tudo atomicamente
            await _uow.SaveChangesAsync(cancellationToken);
            await _uow.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Entrada registrada com sucesso. Transação ID {TransacaoId}.", transacao.Id);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(cancellationToken);
            _logger.LogError("Falha ao registrar entrada. Transação revertida.");
            throw;
        }
    }

    public async Task RegistrarSaidaAsync(
        int vinhoId, int quantidade, decimal valorUnitario,
        string? descricao = null, int? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Registrando SAÍDA: Vinho {VinhoId}, Qtd {Quantidade}...",
            vinhoId, quantidade);

        await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Busca lotes com saldo disponível (FIFO: recebimento mais antigo primeiro)
            var lotes = (await _uow.Lotes.GetLotesAtivosPorVinhoAsync(vinhoId, cancellationToken))
                        .OrderBy(l => l.DataRecebimento)
                        .ToList();

            if (!lotes.Any())
                throw new InvalidOperationException($"Nenhum lote disponível para o vinho ID {vinhoId}.");

            int quantidadeRestante = quantidade;

            // 2. Baixa nos lotes em ordem FIFO
            foreach (var lote in lotes)
            {
                if (quantidadeRestante <= 0) break;

                int baixa = Math.Min(quantidadeRestante, lote.Quantidade);
                lote.RemoverQuantidade(baixa);
                await _uow.Lotes.UpdateAsync(lote, cancellationToken);

                // Cria transação de saída para este lote
                var transacao = new TransacaoEstoque(
                    TipoMovimentacao.Saida,
                    vinhoId,
                    baixa,
                    valorUnitario,
                    descricao ?? "Saída de mercadoria",
                    lote.Id,
                    usuarioId);

                await _uow.Transacoes.AddAsync(transacao, cancellationToken);
                quantidadeRestante -= baixa;
            }

            if (quantidadeRestante > 0)
                throw new InvalidOperationException(
                    $"Estoque insuficiente. Faltam {quantidadeRestante} unidades para o vinho ID {vinhoId}.");

            // 3. Persiste tudo atomicamente
            await _uow.SaveChangesAsync(cancellationToken);
            await _uow.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Saída registrada com sucesso. {Quantidade} unidades baixadas.", quantidade);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(cancellationToken);
            _logger.LogError("Falha ao registrar saída. Transação revertida.");
            throw;
        }
    }

    // ========================
    //      RELATÓRIOS
    // ========================

    public async Task<decimal> GetValorTotalEstoqueAsync(CancellationToken cancellationToken = default)
    {
        var lotes = await _uow.Lotes.GetAllAsync(cancellationToken);
        return lotes.Where(l => l.Ativo)
                    .Sum(l => l.Quantidade * l.PrecoCusto);
    }

    public async Task<int> GetQuantidadeTotalGarrafasAsync(CancellationToken cancellationToken = default)
    {
        var lotes = await _uow.Lotes.GetAllAsync(cancellationToken);
        return lotes.Where(l => l.Ativo).Sum(l => l.Quantidade);
    }
}
