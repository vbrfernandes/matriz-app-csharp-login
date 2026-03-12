
using MatrizApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adicionar o Entity Framework e o SQLite (Adicione esta parte!)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tarefas.db") // Nome do arquivo do banco que será criado
);

// Adiciona os nossos Controllers (nossos "Garçons")
builder.Services.AddControllers();

// 1. CONFIGURAÇÃO DO CORS: Dizemos para o C# aceitar pedidos de fora
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy.AllowAnyOrigin()   // Permite qualquer site (ex: localhost:5500)
              .AllowAnyHeader()   // Permite qualquer tipo de dado (ex: JSON)
              .AllowAnyMethod();  // Permite qualquer ação (GET, POST, DELETE, etc)
    });
});

var app = builder.Build();

// Força a usar HTTPS
app.UseHttpsRedirection();

// 2. ATIVAR O CORS: Liga a regra que criamos ali em cima
app.UseCors("PermitirTudo");

app.UseAuthorization();
app.MapControllers();
app.Run();