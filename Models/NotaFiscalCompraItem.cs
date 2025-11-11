using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstoque.Api.Models
{
    // Esta é a "linha" da nota fiscal
    public class NotaFiscalCompraItem
    {
        public int Id { get; set; }

        // Chave Estrangeira para o cabeçalho da Nota Fiscal
        public int NotaFiscalCompraId { get; set; }
        public virtual NotaFiscalCompra NotaFiscalCompra { get; set; } = null!;

        // Chave Estrangeira para o Produto que está entrando
        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; } = null!;

        // Quantidade que está entrando no estoque
        public int Quantidade { get; set; }

        // O preço de custo unitário deste item nesta nota específica
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecoCustoUnitario { get; set; }
    }
}