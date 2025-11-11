using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstoque.Api.Models
{
    // A "linha" do pedido (tabela de junção)
    public class PedidoItem
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }
        public virtual Pedido Pedido { get; set; } = null!;

        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; } = null!;

        public int Quantidade { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecoUnitarioVenda { get; set; } // "Congela" o preço
    }
}