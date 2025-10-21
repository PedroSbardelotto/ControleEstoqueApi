namespace ControleEstoque.Api.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public int QuantidadeProduto { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
    }
}