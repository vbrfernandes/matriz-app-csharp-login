
using MatrizApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tarefas.db") 
);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy.AllowAnyOrigin()   
              .AllowAnyHeader()   
              .AllowAnyMethod(); 
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("PermitirTudo");

app.UseAuthorization();
app.MapControllers();
app.Run();