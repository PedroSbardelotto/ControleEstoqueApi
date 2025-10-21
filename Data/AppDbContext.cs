using ControleEstoque.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Api.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        public DbSet<User> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Chama a implementação base

            // Configura a coluna 'Preco' na tabela 'Produtos'
            modelBuilder.Entity<Produto>(entity =>
            {
                // Define o tipo da coluna como decimal(18, 2)
                // 18 = total de dígitos permitidos
                // 2 = dígitos após a vírgula (para centavos)
                entity.Property(p => p.Preco).HasColumnType("decimal(18, 2)");
            });
        }
    }
}