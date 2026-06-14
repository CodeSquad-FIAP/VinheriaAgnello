using VinheriaAgnello.Domain.Entities;

namespace VinheriaAgnello.Domain.Interfaces;

/// <summary>
/// Interface para Unit of Work — consolida transações multi-repositório
/// em um único commit atômico.
/// <para>Expõe todos os repositórios da aplicação para que a camada de serviço
/// possa coordenar operações entre múltiplas entidades em uma transação.</para>
/// <para>No contexto mobile, essa abstração permite substituir EF Core + SQLite
/// por Room (Android) ou CoreData (iOS) sem alterar a lógica de negócio.</para>
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IVinhoRepository Vinhos { get; }
    ILoteRepository Lotes { get; }
    ITransacaoEstoqueRepository Transacoes { get; }
    IRepository<Fornecedor> Fornecedores { get; }
    IRepository<Categoria> Categorias { get; }

    /// <summary>Persiste todas as alterações pendentes no banco de dados.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Inicia uma transação explícita.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Confirma a transação atual.</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Reverte a transação atual.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
