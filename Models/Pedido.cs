using System;
using System.Collections.Generic;

namespace ControleEstoque.Api.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; } = null!;

        public DateTime DataPedido { get; set; }
        public string Status { get; set; } = string.Empty;

        // Um Pedido tem uma coleção de Itens de Pedido
        public virtual ICollection<PedidoItem> PedidoItens { get; set; } = new List<PedidoItem>();
    }
}