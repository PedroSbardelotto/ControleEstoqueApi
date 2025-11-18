using System;

namespace ControleEstoque.Api.DTOs
{
    // DTO focado para a tela de "Listagem de Pedidos"
    public class PedidoListDto
    {
        public int Id { get; set; } // Número do Pedido
        public DateTime DataPedido { get; set; }
        //public string Status { get; set; } = string.Empty;
        public string NomeCliente { get; set; } = string.Empty;

        // O campo calculado que você precisa
        public decimal ValorTotal { get; set; }
        public string Status { get; set; }
    }
}