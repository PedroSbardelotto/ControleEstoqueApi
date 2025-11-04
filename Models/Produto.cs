using System.Collections.Generic;
namespace ControleEstoque.Api.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Preco { get; set; }

        // Esta propriedade não precisa mudar, pois é apenas de leitura (get)
        public string Status
        {
            get
            {
                return Quantidade > 0 ? "Em Estoque" : "Sem Estoque";
            }
        }
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}