namespace ControleEstoque.Api.DTOs
{
    public class VisaoGeralEstoqueDto
    {
        public int TotalItensUnicos { get; set; }

        // Renomeado (era ValorTotalEstoque)
        public decimal ValorTotalEstoqueVenda { get; set; }

        // Adicionado
        public decimal ValorTotalEstoqueCusto { get; set; }
    }
}