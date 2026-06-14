using VinheriaAgnello.Domain.Entities;

namespace VinheriaAgnello.Domain.Interfaces;

/// <summary>
/// Interface genérica de repositório (padrão Repository).
/// Define operações CRUD assíncronas comuns a todas as entidades.
/// <para><strong>Por que assíncrono?</strong> Operações de I/O em SQLite
/// (como em qualquer banco) devem ser non-blocking para não travar a thread
/// da API. Em mobile, o mesmo padrão é usado com Room (Kotlin) ou CoreData (Swift).</para>
/// </summary>
/// <typeparam name="T">Tipo da entidade de domínio.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Obtém uma entidade pelo ID.</summary>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Obtém todas as entidades.</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Adiciona uma nova entidade ao repositório.</summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Atualiza uma entidade existente.</summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Remove uma entidade pelo ID.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
