using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Api.DTOs
{
    // Representa o pedido completo que o front-end envia
    public class PedidoCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ClienteId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "O pedido deve conter pelo menos um item.")]
        public List<PedidoItemCreateDto> Itens { get; set; } = new List<PedidoItemCreateDto>();
    }
}