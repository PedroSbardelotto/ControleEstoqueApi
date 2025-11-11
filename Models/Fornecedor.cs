namespace ControleEstoque.Api.Models
{
    public class Fornecedor
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty; // Ex: "Papelaria XYZ Ltda"
        public string Cnpj { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public virtual ICollection<NotaFiscalCompra> NotasFiscaisCompra { get; set; } = new List<NotaFiscalCompra>();
    }
}