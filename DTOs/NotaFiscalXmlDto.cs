// DTOs/NotaFiscalXmlDto.cs
namespace ControleEstoque.Api.DTOs
{
    public class NotaFiscalXmlDto
    {
        public string NumeroNota { get; set; }
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; }
        public string FornecedorCnpj { get; set; }
        public DateTime DataEmissao { get; set; }
        public decimal ValorTotal { get; set; }
        public List<NotaFiscalItemXmlDto> Itens { get; set; } = new List<NotaFiscalItemXmlDto>();
    }

    public class NotaFiscalItemXmlDto
    {
        public string CodigoProduto { get; set; }
        public string NomeProduto { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoCustoUnitario { get; set; }
    }
}