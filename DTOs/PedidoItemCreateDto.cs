using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Api.DTOs
{
    // Representa um item no "carrinho de compras"
    public class PedidoItemCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ProdutoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }
    }
}