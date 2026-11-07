
 // Microsserviço Catálogo de Vinhos — API mínima, sem dependências externas.
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);

    var caminhoCertificado = Environment.GetEnvironmentVariable("TLS_CERT_PATH");
    var senhaCertificado = Environment.GetEnvironmentVariable("TLS_CERT_PASSWORD");

    if (!string.IsNullOrWhiteSpace(caminhoCertificado) &&
        !string.IsNullOrWhiteSpace(senhaCertificado))
    {
        options.ListenAnyIP(8443, listen =>
            listen.UseHttps(caminhoCertificado, senhaCertificado));
    }
});

var segredo = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET não configurada.");

var clientIdEsperado = Environment.GetEnvironmentVariable("JWT_CLIENT_ID")
    ?? throw new InvalidOperationException("JWT_CLIENT_ID não configurado.");

var senhaEsperada = Environment.GetEnvironmentVariable("JWT_CLIENT_PASSWORD")
    ?? throw new InvalidOperationException("JWT_CLIENT_PASSWORD não configurada.");

var chave = Convert.FromBase64String(segredo);

if (chave.Length < 32)
{
    throw new InvalidOperationException("A chave JWT deve possuir pelo menos 32 bytes.");
}

const string Emissor = "vinheria-catalogo";
const string Audiencia = "vinheria-catalogo";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,

            ValidIssuer = Emissor,
            ValidAudience = Audiencia,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

            IssuerSigningKey = new SymmetricSecurityKey(chave),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

const string Versao = "1.1.0";

Vinho[] vinhos =
[
    new(1, "Agnello Reserva Tinto", "Tinto", 2019, 189.90m, 24),
    new(2, "Agnello Branco Seco", "Branco", 2021, 129.90m, 18),
    new(3, "Villa Agnello Rosé", "Rosé", 2022, 99.90m, 31),
    new(4, "Agnello Espumante Brut", "Espumante", 2020, 219.90m, 12),
];

app.MapPost("/auth/login", (LoginRequest login) =>
{
    bool idValido = string.Equals(
        login.ClientId,
        clientIdEsperado,
        StringComparison.Ordinal
    );

    bool senhaValida = CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(login.Password ?? string.Empty),
        Encoding.UTF8.GetBytes(senhaEsperada)
    );

    if (!idValido || !senhaValida)
    {
        return Results.Unauthorized();
    }

    var agora = DateTime.UtcNow;

    var token = new JwtSecurityToken(
        issuer: Emissor,
        audience: Audiencia,
        claims:
        [
            new Claim(JwtRegisteredClaimNames.Sub, clientIdEsperado),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ],
        notBefore: agora,
        expires: agora.AddMinutes(10),
        signingCredentials: new SigningCredentials(
            new SymmetricSecurityKey(chave),
            SecurityAlgorithms.HmacSha256
        )
    );

    var tokenGerado = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new
    {
        access_token = tokenGerado,
        token_type = "Bearer",
        expires_in = 600
    });
}).AllowAnonymous();

app.MapGet("/vinhos", () =>
    Results.Ok(new
    {
        servico = "catalogo",
        versao = Versao,
        total = vinhos.Length,
        itens = vinhos
    })
).RequireAuthorization();

app.MapGet("/vinhos/{id:int}", (int id) =>
{
    var vinho = vinhos.FirstOrDefault(v => v.Id == id);

    return vinho is null
        ? Results.NotFound(new { erro = $"vinho {id} nao encontrado" })
        : Results.Ok(vinho);
}).RequireAuthorization();

app.MapGet("/health", () =>
    Results.Ok(new
    {
        servico = "catalogo",
        versao = Versao,
        status = "ok",
        host = Environment.MachineName
    })
);

app.Run();

record Vinho(int Id, string Nome, string Tipo, int Safra, decimal Preco, int Estoque);

record LoginRequest(string? ClientId, string? Password);
