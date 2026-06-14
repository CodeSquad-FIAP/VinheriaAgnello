using Microsoft.EntityFrameworkCore;
using VinheriaAgnello.Domain.Entities;

namespace VinheriaAgnello.Infrastructure.Data;

/// <summary>
/// Contexto do Entity Framework Core para a Vinheria Agnello.
/// Utiliza SQLite como backend de persistência, compatível com o modelo
/// de dados do aplicativo mobile (ambos SQLite, mesmos tipos de coluna).
/// <para>Configuração via Fluent API em <c>OnModelCreating</c> para controle
/// fino dos tipos de coluna, índices e relacionamentos.</para>
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // --- DbSets ---
    public DbSet<Vinho> Vinhos => Set<Vinho>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<TransacaoEstoque> TransacoesEstoque => Set<TransacaoEstoque>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Vinho ---
        modelBuilder.Entity<Vinho>(entity =>
        {
            entity.ToTable("Vinhos");
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Nome)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(v => v.CodigoSku)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(v => v.Origem)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(v => v.Descricao)
                  .HasMaxLength(1000);

            entity.Property(v => v.Preco)
                  .IsRequired()
                  .HasColumnType("decimal(10,2)");

            entity.Property(v => v.TeorAlcoolico)
                  .HasColumnType("real");

            entity.Property(v => v.UrlImagem)
                  .HasMaxLength(500);

            entity.Property(v => v.DataCadastro)
                  .HasColumnType("datetime");

            // Índices para consultas comuns
            entity.HasIndex(v => v.CodigoSku).IsUnique();
            entity.HasIndex(v => v.Nome);
            entity.HasIndex(v => v.Ativo);

            // Relacionamento com Categoria
            entity.HasOne(v => v.Categoria)
                  .WithMany(c => c.Vinhos)
                  .HasForeignKey(v => v.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Categoria ---
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Nome)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.Descricao)
                  .HasMaxLength(500);

            entity.HasIndex(c => c.Nome).IsUnique();
        });

        // --- Fornecedor ---
        modelBuilder.Entity<Fornecedor>(entity =>
        {
            entity.ToTable("Fornecedores");
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Nome)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(f => f.Cnpj)
                  .HasMaxLength(18);

            entity.Property(f => f.Contato)
                  .HasMaxLength(100);

            entity.Property(f => f.Telefone)
                  .HasMaxLength(20);

            entity.Property(f => f.Email)
                  .HasMaxLength(150);

            entity.Property(f => f.Endereco)
                  .HasMaxLength(300);

            entity.Property(f => f.DataCadastro)
                  .HasColumnType("datetime");

            entity.HasIndex(f => f.Cnpj);
        });

        // --- Lote ---
        modelBuilder.Entity<Lote>(entity =>
        {
            entity.ToTable("Lotes");
            entity.HasKey(l => l.Id);

            entity.Property(l => l.NumeroLote)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(l => l.PrecoCusto)
                  .IsRequired()
                  .HasColumnType("decimal(10,2)");

            entity.Property(l => l.LocalizacaoDeposito)
                  .HasMaxLength(100);

            entity.Property(l => l.Observacao)
                  .HasMaxLength(500);

            entity.Property(l => l.DataRecebimento)
                  .HasColumnType("datetime");

            entity.Property(l => l.DataValidade)
                  .HasColumnType("datetime");

            // Relacionamento com Vinho
            entity.HasOne(l => l.Vinho)
                  .WithMany(v => v.Lotes)
                  .HasForeignKey(l => l.VinhoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com Fornecedor
            entity.HasOne(l => l.Fornecedor)
                  .WithMany(f => f.Lotes)
                  .HasForeignKey(l => l.FornecedorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(l => l.NumeroLote);
            entity.HasIndex(l => l.VinhoId);
        });

        // --- TransacaoEstoque ---
        modelBuilder.Entity<TransacaoEstoque>(entity =>
        {
            entity.ToTable("TransacoesEstoque");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.TipoMovimentacao)
                  .IsRequired()
                  .HasConversion<int>(); // Enum → INTEGER

            entity.Property(t => t.ValorUnitario)
                  .IsRequired()
                  .HasColumnType("decimal(10,2)");

            entity.Property(t => t.Descricao)
                  .HasMaxLength(300);

            entity.Property(t => t.DataMovimentacao)
                  .HasColumnType("datetime");

            // Relacionamento com Vinho
            entity.HasOne(t => t.Vinho)
                  .WithMany(v => v.Transacoes)
                  .HasForeignKey(t => t.VinhoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento opcional com Lote
            entity.HasOne(t => t.Lote)
                  .WithMany(l => l.Transacoes)
                  .HasForeignKey(t => t.LoteId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(t => t.VinhoId);
            entity.HasIndex(t => t.DataMovimentacao);
        });

        // --- Seed Data ---
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Categorias
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria("Tinto", "Vinhos tintos encorpados e de mesa.") { Id = 1 },
            new Categoria("Branco", "Vinhos brancos leves e estruturados.") { Id = 2 },
            new Categoria("Rosé", "Vinhos rosé frescos e versáteis.") { Id = 3 },
            new Categoria("Espumante", "Espumantes e frisantes.") { Id = 4 },
            new Categoria("Sobremesa", "Vinhos doces de sobremesa.") { Id = 5 }
        );

        // Fornecedores
        modelBuilder.Entity<Fornecedor>().HasData(
            new Fornecedor("Cantina Toscana Ltda.", "12.345.678/0001-90", "Giulio Bianca",
                           "+55 11 99999-0001", "giulio@cantinatoscana.com",
                           "Rua das Vinícolas, 100 - Toscana, IT") { Id = 1 },
            new Fornecedor("Mendoza Wines S.A.", "98.765.432/0001-10", "Carlos Mendoza",
                           "+55 11 99999-0002", "carlos@mendozawines.com",
                           "Av. Uvas, 500 - Mendoza, AR") { Id = 2 },
            new Fornecedor("Provence Rosé Importadora", "11.222.333/0001-55", "Marie Dupont",
                           "+55 11 99999-0003", "marie@provencerose.com",
                           "Rue de la Rose, 42 - Provence, FR") { Id = 3 }
        );

        // Vinhos (seed compatível com os dados Java existentes)
        modelBuilder.Entity<Vinho>().HasData(
            new Vinho("Brunello di Montalcino", "SKU-BRU-001", 1, "Toscana, IT",
                      429.00m, 14.5, 750, 2018, true,
                      "Vinho tinto encorpado com notas de frutas vermelhas e taninos elegantes.") { Id = 1 },
            new Vinho("Chardonnay Reserva", "SKU-CHA-002", 2, "Mendoza, AR",
                      145.00m, 13.0, 750, 2021, true,
                      "Vinho branco com notas de frutas tropicais e leve defumado.") { Id = 2 },
            new Vinho("Côtes de Provence", "SKU-PRO-003", 3, "Provence, FR",
                      180.00m, 12.5, 750, 2022, true,
                      "Rosé fresco e frutado, ideal para dias quentes.") { Id = 3 }
        );
    }
}
