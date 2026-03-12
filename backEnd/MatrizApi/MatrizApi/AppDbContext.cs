using MatrizApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MatrizApi
{
    // O DbContext é a ponte entre o seu código C# e o Banco de Dados SQL
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aqui nós dizemos: "Crie uma tabela chamada 'Tarefas' baseada na classe 'Tarefa'"
        public DbSet<Tarefa> Tarefas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}