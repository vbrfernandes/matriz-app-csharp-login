using MatrizApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MatrizApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Tarefa> Tarefas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura o ID para ser auto-incremento real no Postgres
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Id)
                .UseIdentityByDefaultColumn();

            modelBuilder.Entity<Tarefa>()
                .Property(t => t.Id)
                .UseIdentityByDefaultColumn();
        }
    }
}