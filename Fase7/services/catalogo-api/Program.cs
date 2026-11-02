// Microsserviço Catálogo de Vinhos — API mínima, sem dependências externas.
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");
var app = builder.Build();

const string Versao = "1.1.0";

Vinho[] vinhos =
[
    new(1, "Agnello Reserva Tinto", "Tinto", 2019, 189.90m, 24),
    new(2, "Agnello Branco Seco", "Branco", 2021, 129.90m, 18),
    new(3, "Villa Agnello Rosé", "Rosé", 2022, 99.90m, 31),
    new(4, "Agnello Espumante Brut", "Espumante", 2020, 219.90m, 12),
];

app.MapGet("/vinhos", () => Results.Ok(new { servico = "catalogo", versao = Versao, total = vinhos.Length, itens = vinhos }));

app.MapGet("/vinhos/{id:int}", (int id) =>
{
    var vinho = vinhos.FirstOrDefault(v => v.Id == id);
    return vinho is null ? Results.NotFound(new { erro = $"vinho {id} nao encontrado" }) : Results.Ok(vinho);
});

app.MapGet("/health", () => Results.Ok(new { servico = "catalogo", versao = Versao, status = "ok", host = Environment.MachineName }));

app.Run();

record Vinho(int Id, string Nome, string Tipo, int Safra, decimal Preco, int Estoque);
