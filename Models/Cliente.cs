using System.Collections.Generic; // Adicionar este using

namespace ControleEstoque.Api.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;

        // Propriedade de Navegação para os Pedidos deste Cliente (virtual)
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}