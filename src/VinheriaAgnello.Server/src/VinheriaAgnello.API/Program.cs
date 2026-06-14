using Microsoft.EntityFrameworkCore;
using VinheriaAgnello.Infrastructure;
using VinheriaAgnello.Infrastructure.Data;
using VinheriaAgnello.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Serviços ---

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ISO 8601 para DateTime — compatível com mobile
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Vinheria Agnello - API de Estoque", Version = "v1" });
});

// CORS — permite chamadas do mobile e do frontend web
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Infraestrutura: EF Core + SQLite + Repositórios
var connectionString = builder.Configuration.GetConnectionString("Sqlite")
    ?? "Data Source=vinheria_agnello.db";
builder.Services.AddInfrastructure(connectionString);

// Serviços de aplicação
builder.Services.AddApplicationServices();

var app = builder.Build();

// --- Pipeline ---

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// --- Inicialização do banco de dados ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated(); // Cria o banco SQLite + seed se não existir
}

app.Run();
