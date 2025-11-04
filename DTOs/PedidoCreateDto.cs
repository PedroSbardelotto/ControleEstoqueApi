using System.ComponentModel.DataAnnotations; // Para atributos de validação

namespace ControleEstoque.Api.DTOs
{
    public class PedidoCreateDto
    {
        [Required(ErrorMessage = "O ID do produto é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID do produto inválido.")]
        public int ProdutoId { get; set; }

        [Required(ErrorMessage = "O ID do cliente é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID do cliente inválido.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int QuantidadeProduto { get; set; }
    }
}