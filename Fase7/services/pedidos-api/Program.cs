
 // Microsserviço de Pedidos — consulta o Catálogo PELO NOME DNS (não pelo IP),
 // que é a prova de que os serviços se enxergam por nome dentro da rede da Vinheria.
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");
var app = builder.Build();

const string Versao = "1.1.0";
const string CatalogoHost = "catalogo.vinheria.local";
const int CatalogoPorta = 8443;

var clientId = Environment.GetEnvironmentVariable("JWT_CLIENT_ID")
    ?? throw new InvalidOperationException("JWT_CLIENT_ID não configurado.");

var clientPassword = Environment.GetEnvironmentVariable("JWT_CLIENT_PASSWORD")
    ?? throw new InvalidOperationException("JWT_CLIENT_PASSWORD não configurada.");

var http = new HttpClient
{
    BaseAddress = new Uri($"https://{CatalogoHost}:{CatalogoPorta}"),
    Timeout = TimeSpan.FromSeconds(10)
};

app.MapGet("/pedidos", async () =>
{
    // resolução de nome feita explicitamente para exibir o IP obtido pelo DNS interno
    var enderecos = await Dns.GetHostAddressesAsync(CatalogoHost);
    var ipCatalogo = enderecos.FirstOrDefault(a =>
        a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();

    using var respostaLogin = await http.PostAsJsonAsync("/auth/login", new
    {
        clientId,
        password = clientPassword
    });

    if (!respostaLogin.IsSuccessStatusCode)
    {
        return Results.Problem(
            "Falha na autenticação com o Catálogo.",
            statusCode: 502
        );
    }

    var login = await respostaLogin.Content.ReadFromJsonAsync<JsonElement>();

    if (!login.TryGetProperty("access_token", out var tokenJson) ||
        string.IsNullOrWhiteSpace(tokenJson.GetString()))
    {
        return Results.Problem(
            "O Catálogo não retornou um JWT válido.",
            statusCode: 502
        );
    }

    var token = tokenJson.GetString();

    using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/vinhos");

    requisicao.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    using var respostaCatalogo = await http.SendAsync(requisicao);

    if (!respostaCatalogo.IsSuccessStatusCode)
    {
        return Results.Problem(
            "Falha ao consultar o Catálogo.",
            statusCode: 502
        );
    }

    var resposta = await respostaCatalogo.Content.ReadFromJsonAsync<JsonElement>();

    if (!resposta.TryGetProperty("itens", out var itens) ||
        itens.ValueKind != JsonValueKind.Array)
    {
        return Results.Problem(
            "Resposta inválida do Catálogo.",
            statusCode: 502
        );
    }

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
        catalogo = new
        {
            url = $"https://{CatalogoHost}:{CatalogoPorta}",
            resolvido_por_dns = ipCatalogo,
            vinhos_disponiveis = itens.GetArrayLength()
        },
        total = pedidos.Length,
        itens = pedidos,
    });
});

app.MapGet("/health", () => Results.Ok(new
{
    servico = "pedidos",
    versao = Versao,
    status = "ok",
    host = Environment.MachineName
}));

app.Run();
