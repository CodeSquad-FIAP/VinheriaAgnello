using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinheriaAgnello.Domain.Entities;

/// <summary>
/// Fornecedor de vinhos da Vinheria Agnello.
/// <para><strong>Alinhamento Mobile:</strong> Id como INTEGER, demais campos como TEXT.
/// CNPJ armazenado como string (sem formatação) para evitar perda de leading zeros
/// e maximizar compatibilidade com Android ContentValues e iOS NSUserDefaults.</para>
/// </summary>
public class Fornecedor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public string Nome { get; private set; } = string.Empty;
    public string? Cnpj { get; private set; }
    public string? Contato { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Endereco { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime DataCadastro { get; private set; }

    public ICollection<Lote> Lotes { get; private set; } = new List<Lote>();

    private Fornecedor() { }

    public Fornecedor(string nome, string? cnpj = null, string? contato = null,
                      string? telefone = null, string? email = null, string? endereco = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do fornecedor é obrigatório.", nameof(nome));

        Nome = nome;
        Cnpj = cnpj;
        Contato = contato;
        Telefone = telefone;
        Email = email;
        Endereco = endereco;
        DataCadastro = DateTime.UtcNow;
    }

    public void AtualizarContato(string? contato, string? telefone, string? email)
    {
        Contato = contato;
        Telefone = telefone;
        Email = email;
    }

    public void Desativar() => Ativo = false;
}
