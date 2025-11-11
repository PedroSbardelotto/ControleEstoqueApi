using System.Collections.Generic;
namespace ControleEstoque.Api.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal PrecoVenda { get; set; }
        public decimal PrecoCusto { get; set; }

        // Esta propriedade não precisa mudar, pois é apenas de leitura (get)
        public string Status
        {
            get
            {
                return Quantidade > 0 ? "Em Estoque" : "Sem Estoque";
            }
        }
        public virtual ICollection<PedidoItem> PedidoItens { get; set; } = new List<PedidoItem>();
        public virtual ICollection<NotaFiscalCompraItem> NotaFiscalCompraItens { get; set; } = new List<NotaFiscalCompraItem>();
    }
}