using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VinheriaAgnello.Domain.Enums;

namespace VinheriaAgnello.Domain.Entities;

/// <summary>
/// Registro de movimentação de estoque (entrada ou saída).
/// <para><strong>Alinhamento Mobile:</strong> TipoMovimentacao como INTEGER (0/1),
/// Quantidade como INTEGER, ValorUnitario como DECIMAL(10,2),
/// DataMovimentacao como TEXT (ISO 8601).</para>
/// </summary>
public class TransacaoEstoque
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public TipoMovimentacao TipoMovimentacao { get; private set; }

    public int VinhoId { get; private set; }
    public Vinho? Vinho { get; private set; }

    public int? LoteId { get; private set; }
    public Lote? Lote { get; private set; }

    /// <summary>Quantidade de garrafas movimentadas.</summary>
    public int Quantidade { get; private set; }

    /// <summary>Valor unitário praticado na transação.</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal ValorUnitario { get; private set; }

    /// <summary>Data e hora da movimentação.</summary>
    public DateTime DataMovimentacao { get; private set; }

    /// <summary>Descrição ou motivo da movimentação (ex: "Venda balcão", "Ajuste de inventário").</summary>
    public string? Descricao { get; private set; }

    /// <summary>ID opcional do usuário que registrou a movimentação.</summary>
    public int? UsuarioId { get; private set; }

    private TransacaoEstoque() { }

    public TransacaoEstoque(
        TipoMovimentacao tipoMovimentacao,
        int vinhoId,
        int quantidade,
        decimal valorUnitario,
        string? descricao = null,
        int? loteId = null,
        int? usuarioId = null)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        if (valorUnitario < 0)
            throw new ArgumentException("Valor unitário não pode ser negativo.", nameof(valorUnitario));

        TipoMovimentacao = tipoMovimentacao;
        VinhoId = vinhoId;
        LoteId = loteId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        Descricao = descricao;
        UsuarioId = usuarioId;
        DataMovimentacao = DateTime.UtcNow;
    }

    /// <summary>Valor total da movimentação (Quantidade × ValorUnitario).</summary>
    public decimal ValorTotal => Quantidade * ValorUnitario;
}
