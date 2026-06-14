using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinheriaAgnello.Domain.Entities;

/// <summary>
/// Representa um vinho comercializado pela Vinheria Agnello.
/// <para>Mapeamento de tipos alinhado com o banco Mobile SQLite:</para>
/// <list type="bullet">
///   <item><c>Id</c> → INTEGER PRIMARY KEY (auto-increment) — mesmo tipo usado no app mobile.</item>
///   <item><c>Preco</c> → DECIMAL(10,2) / REAL — precisão financeira idêntica ao BigDecimal do Java e ao Double do Kotlin.</item>
///   <item><c>TeorAlcoolico</c> → REAL — compatível com Float/Double mobile.</item>
///   <item><c>VolumeMl</c> → INTEGER — inteiro 32 bits, sem ambiguidade entre plataformas.</item>
///   <item><c>AnoSafra</c> → INTEGER — ano como número, não como DateTime, para evitar fuso horário.</item>
///   <item><c>DataCadastro</c> → TEXT (ISO 8601) — DateTime em C#, convertido para string no SQLite.</item>
///   <item><c>EhImportado</c> → INTEGER 0/1 — booleano, compatível com SQLite nativo.</item>
/// </list>
/// </summary>
public class Vinho
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public string Nome { get; private set; } = string.Empty;
    public string CodigoSku { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }

    public int CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }

    public string Origem { get; private set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Preco { get; private set; }

    public double TeorAlcoolico { get; private set; }
    public int VolumeMl { get; private set; }
    public int AnoSafra { get; private set; }
    public bool EhImportado { get; private set; }
    public string? UrlImagem { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public bool Ativo { get; private set; } = true;

    public ICollection<Lote> Lotes { get; private set; } = new List<Lote>();
    public ICollection<TransacaoEstoque> Transacoes { get; private set; } = new List<TransacaoEstoque>();

    private Vinho() { }

    public Vinho(
        string nome,
        string codigoSku,
        int categoriaId,
        string origem,
        decimal preco,
        double teorAlcoolico,
        int volumeMl,
        int anoSafra,
        bool ehImportado,
        string? descricao = null,
        string? urlImagem = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do vinho é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(codigoSku))
            throw new ArgumentException("Código SKU é obrigatório.", nameof(codigoSku));
        if (preco <= 0)
            throw new ArgumentException("Preço deve ser maior que zero.", nameof(preco));
        if (volumeMl <= 0)
            throw new ArgumentException("Volume deve ser maior que zero.", nameof(volumeMl));
        if (anoSafra < 1900 || anoSafra > DateTime.UtcNow.Year + 5)
            throw new ArgumentException("Ano da safra inválido.", nameof(anoSafra));

        Nome = nome;
        CodigoSku = codigoSku;
        CategoriaId = categoriaId;
        Origem = origem;
        Preco = preco;
        TeorAlcoolico = teorAlcoolico;
        VolumeMl = volumeMl;
        AnoSafra = anoSafra;
        EhImportado = ehImportado;
        Descricao = descricao;
        UrlImagem = urlImagem;
        DataCadastro = DateTime.UtcNow;
        Ativo = true;
    }

    public void AtualizarPreco(decimal novoPreco)
    {
        if (novoPreco <= 0)
            throw new ArgumentException("Preço deve ser maior que zero.", nameof(novoPreco));
        Preco = novoPreco;
    }

    public void Descontinuar() => Ativo = false;
}
