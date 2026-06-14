using VinheriaAgnello.Domain.Entities;
using VinheriaAgnello.Domain.Enums;

namespace VinheriaAgnello.Tests;

public class VinhoTests
{
    [Fact]
    public void CriarVinho_ComDadosValidos_DeveInicializarCorretamente()
    {
        var vinho = new Vinho(
            nome: "Brunello di Montalcino",
            codigoSku: "SKU-BRU-001",
            categoriaId: 1,
            origem: "Toscana, IT",
            preco: 429.00m,
            teorAlcoolico: 14.5,
            volumeMl: 750,
            anoSafra: 2018,
            ehImportado: true);

        Assert.Equal("Brunello di Montalcino", vinho.Nome);
        Assert.Equal("SKU-BRU-001", vinho.CodigoSku);
        Assert.Equal(429.00m, vinho.Preco);
        Assert.Equal(14.5, vinho.TeorAlcoolico);
        Assert.True(vinho.EhImportado);
        Assert.True(vinho.Ativo);
        Assert.NotEqual(default, vinho.DataCadastro);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void CriarVinho_ComPrecoInvalido_DeveLancarExcecao(decimal precoInvalido)
    {
        Assert.Throws<ArgumentException>(() => new Vinho(
            "Teste", "SKU-001", 1, "BR", precoInvalido, 12.0, 750, 2020, false));
    }

    [Fact]
    public void AtualizarPreco_ComValorPositivo_DeveAtualizar()
    {
        var vinho = new Vinho("Teste", "SKU-001", 1, "BR", 100m, 12.0, 750, 2020, false);
        vinho.AtualizarPreco(150.50m);
        Assert.Equal(150.50m, vinho.Preco);
    }

    [Fact]
    public void Descontinuar_DeveMarcaComoInativo()
    {
        var vinho = new Vinho("Teste", "SKU-001", 1, "BR", 100m, 12.0, 750, 2020, false);
        vinho.Descontinuar();
        Assert.False(vinho.Ativo);
    }
}

public class LoteTests
{
    [Fact]
    public void AdicionarQuantidade_DeveIncrementarSaldo()
    {
        var lote = new Lote("LOTE-001", 1, 1, 100, 50.00m);
        lote.AdicionarQuantidade(50);
        Assert.Equal(150, lote.Quantidade);
    }

    [Fact]
    public void RemoverQuantidade_ComSaldoSuficiente_DeveDecrementar()
    {
        var lote = new Lote("LOTE-001", 1, 1, 100, 50.00m);
        lote.RemoverQuantidade(30);
        Assert.Equal(70, lote.Quantidade);
    }

    [Fact]
    public void RemoverQuantidade_ComSaldoInsuficiente_DeveLancarExcecao()
    {
        var lote = new Lote("LOTE-001", 1, 1, 10, 50.00m);
        Assert.Throws<InvalidOperationException>(() => lote.RemoverQuantidade(20));
    }
}

public class TransacaoEstoqueTests
{
    [Fact]
    public void CriarTransacaoEntrada_DeveCalcularValorTotal()
    {
        var transacao = new TransacaoEstoque(
            TipoMovimentacao.Entrada, 1, 10, 50.00m,
            "Entrada inicial", loteId: 1);

        Assert.Equal(TipoMovimentacao.Entrada, transacao.TipoMovimentacao);
        Assert.Equal(500.00m, transacao.ValorTotal); // 10 × 50
    }

    [Fact]
    public void ValorTotal_DeveSerQuantidadeVezesValorUnitario()
    {
        var transacao = new TransacaoEstoque(TipoMovimentacao.Saida, 1, 5, 100m);
        Assert.Equal(500m, transacao.ValorTotal);
    }
}

public class FornecedorTests
{
    [Fact]
    public void CriarFornecedor_ComNomeValido_DeveInicializar()
    {
        var f = new Fornecedor("Cantina Toscana Ltda.", "12.345.678/0001-90",
                               "Giulio Bianca", "+55 11 99999-0001");
        Assert.Equal("Cantina Toscana Ltda.", f.Nome);
        Assert.Equal("12.345.678/0001-90", f.Cnpj);
        Assert.True(f.Ativo);
    }

    [Fact]
    public void Desativar_DeveMarcaComoInativo()
    {
        var f = new Fornecedor("Teste");
        f.Desativar();
        Assert.False(f.Ativo);
    }
}
