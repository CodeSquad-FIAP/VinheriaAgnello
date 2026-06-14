using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VinheriaAgnello.Domain.Entities;
using VinheriaAgnello.Domain.Interfaces;

namespace VinheriaAgnello.Infrastructure.Repositories;

/// <summary>
/// Implementação do Unit of Work sobre EF Core.
/// Gerencia transações com commit/rollback explícitos e
/// expõe todos os repositórios da aplicação.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public IVinhoRepository Vinhos { get; }
    public ILoteRepository Lotes { get; }
    public ITransacaoEstoqueRepository Transacoes { get; }
    public IRepository<Fornecedor> Fornecedores { get; }
    public IRepository<Categoria> Categorias { get; }

    public UnitOfWork(
        DbContext context,
        IVinhoRepository vinhoRepository,
        ILoteRepository loteRepository,
        ITransacaoEstoqueRepository transacaoRepository,
        IRepository<Fornecedor> fornecedorRepository,
        IRepository<Categoria> categoriaRepository)
    {
        _context = context;
        Vinhos = vinhoRepository;
        Lotes = loteRepository;
        Transacoes = transacaoRepository;
        Fornecedores = fornecedorRepository;
        Categorias = categoriaRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            await _currentTransaction.CommitAsync(cancellationToken);
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            await _currentTransaction.RollbackAsync(cancellationToken);
        _currentTransaction = null;
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
