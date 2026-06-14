using Microsoft.AspNetCore.Mvc;
using VinheriaAgnello.Domain.Entities;
using VinheriaAgnello.Domain.Interfaces;

namespace VinheriaAgnello.API.Controllers;

/// <summary>
/// Controlador REST para operações de estoque da Vinheria Agnello.
/// <para>Endpoints compatíveis com os tipos de dados do aplicativo mobile —
/// JSON com camelCase, ISO 8601 para datas, decimais sem arredondamento.</para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EstoqueController : ControllerBase
{
    private readonly IEstoqueService _estoqueService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<EstoqueController> _logger;

    public EstoqueController(
        IEstoqueService estoqueService,
        IUnitOfWork uow,
        ILogger<EstoqueController> logger)
    {
        _estoqueService = estoqueService;
        _uow = uow;
        _logger = logger;
    }

    // ========================
    //       VINHOS
    // ========================

    /// <summary>Lista todos os vinhos ativos com saldo em estoque.</summary>
    [HttpGet("vinhos")]
    public async Task<ActionResult<IReadOnlyList<Vinho>>> GetVinhosEmEstoque(CancellationToken cancellationToken)
    {
        var vinhos = await _estoqueService.GetVinhosEmEstoqueAsync(cancellationToken);
        return Ok(vinhos);
    }

    /// <summary>Obtém um vinho pelo ID.</summary>
    [HttpGet("vinhos/{id}")]
    public async Task<ActionResult<Vinho>> GetVinhoPorId(int id, CancellationToken cancellationToken)
    {
        var vinho = await _uow.Vinhos.GetByIdAsync(id, cancellationToken);
        if (vinho is null)
            return NotFound(new { mensagem = $"Vinho ID {id} não encontrado." });
        return Ok(vinho);
    }

    /// <summary>Obtém o saldo em garrafas de um vinho.</summary>
    [HttpGet("vinhos/{vinhoId}/saldo")]
    public async Task<ActionResult<object>> GetSaldoVinho(int vinhoId, CancellationToken cancellationToken)
    {
        var vinho = await _uow.Vinhos.GetByIdAsync(vinhoId, cancellationToken);
        if (vinho is null)
            return NotFound(new { mensagem = $"Vinho ID {vinhoId} não encontrado." });

        var saldo = await _estoqueService.GetSaldoVinhoAsync(vinhoId, cancellationToken);
        return Ok(new { vinhoId, vinhoNome = vinho.Nome, saldoEmGarrafas = saldo });
    }

    // ========================
    //        LOTES
    // ========================

    /// <summary>Lista os lotes disponíveis de um vinho.</summary>
    [HttpGet("vinhos/{vinhoId}/lotes")]
    public async Task<ActionResult<IReadOnlyList<Lote>>> GetLotesDoVinho(int vinhoId, CancellationToken cancellationToken)
    {
        var lotes = await _estoqueService.GetLotesDoVinhoAsync(vinhoId, cancellationToken);
        return Ok(lotes);
    }

    // ========================
    //   MOVIMENTAÇÕES
    // ========================

    /// <summary>Registra uma entrada de mercadoria no estoque.</summary>
    [HttpPost("entrada")]
    public async Task<ActionResult> RegistrarEntrada(
        [FromBody] MovimentacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _estoqueService.RegistrarEntradaAsync(
                request.VinhoId,
                request.LoteId!.Value,
                request.Quantidade,
                request.ValorUnitario,
                request.Descricao,
                request.UsuarioId,
                cancellationToken);

            return Ok(new { mensagem = "Entrada registrada com sucesso." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar entrada.");
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Registra uma saída de mercadoria do estoque.</summary>
    [HttpPost("saida")]
    public async Task<ActionResult> RegistrarSaida(
        [FromBody] MovimentacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _estoqueService.RegistrarSaidaAsync(
                request.VinhoId,
                request.Quantidade,
                request.ValorUnitario,
                request.Descricao,
                request.UsuarioId,
                cancellationToken);

            return Ok(new { mensagem = "Saída registrada com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar saída.");
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Histórico de movimentações de um vinho.</summary>
    [HttpGet("vinhos/{vinhoId}/historico")]
    public async Task<ActionResult<IReadOnlyList<TransacaoEstoque>>> GetHistorico(
        int vinhoId, CancellationToken cancellationToken)
    {
        var historico = await _estoqueService.GetHistoricoVinhoAsync(vinhoId, cancellationToken);
        return Ok(historico);
    }

    // ========================
    //      RELATÓRIOS
    // ========================

    /// <summary>Resumo geral do estoque.</summary>
    [HttpGet("resumo")]
    public async Task<ActionResult> GetResumoEstoque(CancellationToken cancellationToken)
    {
        var totalGarrafas = await _estoqueService.GetQuantidadeTotalGarrafasAsync(cancellationToken);
        var valorTotal = await _estoqueService.GetValorTotalEstoqueAsync(cancellationToken);
        var vinhosEmEstoque = await _estoqueService.GetVinhosEmEstoqueAsync(cancellationToken);

        return Ok(new
        {
            totalVinhosEmEstoque = vinhosEmEstoque.Count,
            totalGarrafas,
            valorTotalEstoque = valorTotal,
            dataConsulta = DateTime.UtcNow
        });
    }
}

// ========================
//   DTOs / Request Models
// ========================

/// <summary>
/// Modelo de requisição para movimentações de estoque.
/// <para><strong>Alinhamento Mobile:</strong> todos os campos em camelCase,
/// decimais como número, inteiros para IDs.</para>
/// </summary>
public class MovimentacaoRequest
{
    /// <summary>ID do vinho.</summary>
    public int VinhoId { get; set; }

    /// <summary>ID do lote (obrigatório para entrada, opcional para saída).</summary>
    public int? LoteId { get; set; }

    /// <summary>Quantidade de garrafas.</summary>
    public int Quantidade { get; set; }

    /// <summary>Valor unitário da movimentação.</summary>
    public decimal ValorUnitario { get; set; }

    /// <summary>Descrição ou motivo da movimentação.</summary>
    public string? Descricao { get; set; }

    /// <summary>ID do usuário que registrou (opcional).</summary>
    public int? UsuarioId { get; set; }
}
