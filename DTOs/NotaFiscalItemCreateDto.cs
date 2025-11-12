namespace ControleEstoque.Api.DTOs
{
    public class NotaFiscalItemCreateDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoCustoUnitario { get; set; }
    }
}