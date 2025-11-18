namespace ControleEstoque.Api.DTOs
{
    /// <summary>
    /// DTO para o relatório de itens sem movimento (parados).
    /// </summary>
    public class ItemParadoDto
    {
        public string NomeProduto { get; set; }
        public int DiasSemMovimento { get; set; }
        public int QuantidadeEstoque { get; set; }
    }
}