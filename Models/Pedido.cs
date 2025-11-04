namespace ControleEstoque.Api.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; } = null!;
        public int ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; } = null!;
        public int QuantidadeProduto { get; set; }

        
    }
}