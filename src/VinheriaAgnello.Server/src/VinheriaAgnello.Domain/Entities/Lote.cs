using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinheriaAgnello.Domain.Entities;

/// <summary>
/// Representa um lote de vinhos no estoque da Vinheria Agnello.
/// <para>Cada lote vincula um <see cref="Vinho"/> a um <see cref="Fornecedor"/>,
/// controlando quantidade, custo de aquisição e rastreabilidade por número de lote.</para>
/// <para><strong>Alinhamento Mobile:</strong> PrecoCusto como DECIMAL(10,2),
/// Quantidade como INTEGER, DataRecebimento como TEXT (ISO 8601).</para>
/// </summary>
public class Lote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    /// <summary>Número ou código identificador do lote junto ao fornecedor.</summary>
    public string NumeroLote { get; private set; } = string.Empty;

    public int VinhoId { get; private set; }
    public Vinho? Vinho { get; private set; }

    public int FornecedorId { get; private set; }
    public Fornecedor? Fornecedor { get; private set; }

    /// <summary>Quantidade de garrafas neste lote.</summary>
    public int Quantidade { get; private set; }

    /// <summary>Preço de custo unitário pago ao fornecedor.</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoCusto { get; private set; }

    /// <summary>Data de recebimento do lote.</summary>
    public DateTime DataRecebimento { get; private set; }

    /// <summary>Data de validade (se aplicável para vinhos de prateleira curta).</summary>
    public DateTime? DataValidade { get; private set; }

    /// <summary>Localização física no depósito (ex: "Prateleira A3").</summary>
    public string? LocalizacaoDeposito { get; private set; }

    /// <summary>Observações sobre o lote.</summary>
    public string? Observacao { get; private set; }

    public bool Ativo { get; private set; } = true;

    public ICollection<TransacaoEstoque> Transacoes { get; private set; } = new List<TransacaoEstoque>();

    private Lote() { }

    public Lote(string numeroLote, int vinhoId, int fornecedorId, int quantidade,
                decimal precoCusto, DateTime? dataValidade = null,
                string? localizacaoDeposito = null, string? observacao = null)
    {
        if (string.IsNullOrWhiteSpace(numeroLote))
            throw new ArgumentException("Número do lote é obrigatório.", nameof(numeroLote));
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        if (precoCusto <= 0)
            throw new ArgumentException("Preço de custo deve ser maior que zero.", nameof(precoCusto));

        NumeroLote = numeroLote;
        VinhoId = vinhoId;
        FornecedorId = fornecedorId;
        Quantidade = quantidade;
        PrecoCusto = precoCusto;
        DataRecebimento = DateTime.UtcNow;
        DataValidade = dataValidade;
        LocalizacaoDeposito = localizacaoDeposito;
        Observacao = observacao;
    }

    /// <summary>Adiciona quantidade ao lote (entrada complementar).</summary>
    public void AdicionarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade a adicionar deve ser positiva.", nameof(quantidade));
        Quantidade += quantidade;
    }

    /// <summary>Remove quantidade do lote (saída/baixa).</summary>
    public void RemoverQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade a remover deve ser positiva.", nameof(quantidade));
        if (quantidade > Quantidade)
            throw new InvalidOperationException(
                $"Quantidade insuficiente no lote {NumeroLote}. " +
                $"Disponível: {Quantidade}, Solicitada: {quantidade}.");
        Quantidade -= quantidade;
    }
}
