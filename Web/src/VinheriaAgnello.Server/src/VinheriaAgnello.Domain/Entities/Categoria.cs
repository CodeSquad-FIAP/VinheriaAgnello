using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinheriaAgnello.Domain.Entities;

/// <summary>
/// Categoria de vinho (Tinto, Branco, Rosé, Espumante, etc.).
/// <para><strong>Alinhamento Mobile:</strong> Id como INTEGER, Nome como TEXT.
/// Esta entidade funciona como uma tabela de domínio referenciável por chave estrangeira,
/// eliminando strings soltas no código e garantindo integridade referencial.</para>
/// </summary>
public class Categoria
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }

    public ICollection<Vinho> Vinhos { get; private set; } = new List<Vinho>();

    private Categoria() { }

    public Categoria(string nome, string? descricao = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da categoria é obrigatório.", nameof(nome));
        Nome = nome;
        Descricao = descricao;
    }
}
