// Microsserviço de Pedidos — consulta o Catálogo PELO NOME DNS (não pelo IP),
// que é a prova de que os serviços se enxergam por nome dentro da rede da Vinheria.
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");
var app = builder.Build();

const string Versao = "1.1.0";
const string CatalogoHost = "catalogo.vinheria.local";
const int CatalogoPorta = 8080;

var http = new HttpClient { BaseAddress = new Uri($"http://{CatalogoHost}:{CatalogoPorta}"), Timeout = TimeSpan.FromSeconds(10) };

app.MapGet("/pedidos", async () =>
{
    // resolução de nome feita explicitamente para exibir o IP obtido pelo DNS interno
    var enderecos = await Dns.GetHostAddressesAsync(CatalogoHost);
    var ipCatalogo = enderecos.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();

    var resposta = await http.GetFromJsonAsync<JsonElement>("/vinhos");
    var itens = resposta.GetProperty("itens");

    var pedidos = new[]
    {
        new { pedido = 1001, cliente = "Giulio Agnello", vinhoId = 1, quantidade = 2, status = "confirmado" },
        new { pedido = 1002, cliente = "Bianca Agnello", vinhoId = 3, quantidade = 1, status = "separando" },
    };

    return Results.Ok(new
    {
        servico = "pedidos",
        versao = Versao,
        host = Environment.MachineName,
        catalogo = new { url = $"http://{CatalogoHost}:{CatalogoPorta}", resolvido_por_dns = ipCatalogo, vinhos_disponiveis = itens.GetArrayLength() },
        total = pedidos.Length,
        itens = pedidos,
    });
});

app.MapGet("/health", () => Results.Ok(new { servico = "pedidos", versao = Versao, status = "ok", host = Environment.MachineName }));

app.Run();
