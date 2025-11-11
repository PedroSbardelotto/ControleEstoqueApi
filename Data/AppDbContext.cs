using ControleEstoque.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Adiciona a nova tabela/DbSet para os Itens do Pedido
        public DbSet<PedidoItem> PedidoItens { get; set; }

        public DbSet<User> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }

        // Sobrescrevemos este método para configurar os modelos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura a precisão dos decimais na entidade Produto
            modelBuilder.Entity<Produto>(entity =>
            {
                // Coluna PrecoVenda
                entity.Property(p => p.PrecoVenda).HasColumnType("decimal(18, 2)");

                // Coluna PrecoCusto (que adicionamos na Iniciativa 1)
                entity.Property(p => p.PrecoCusto).HasColumnType("decimal(18, 2)");
            });

            // Configura a precisão do decimal na nova entidade PedidoItem
            modelBuilder.Entity<PedidoItem>(entity =>
            {
                // Coluna PrecoUnitarioVenda (para "congelar" o preço)
                entity.Property(p => p.PrecoUnitarioVenda).HasColumnType("decimal(18, 2)");
            });

            // (Não é estritamente necessário, mas bom para garantir)
            // Configura as chaves estrangeiras de PedidoItem
            modelBuilder.Entity<PedidoItem>()
                .HasOne(pi => pi.Pedido)
                .WithMany(p => p.PedidoItens) // A coleção em Pedido.cs
                .HasForeignKey(pi => pi.PedidoId);

            modelBuilder.Entity<PedidoItem>()
                .HasOne(pi => pi.Produto)
                .WithMany(p => p.PedidoItens) // A coleção em Produto.cs
                .HasForeignKey(pi => pi.ProdutoId);
        }
    }
}