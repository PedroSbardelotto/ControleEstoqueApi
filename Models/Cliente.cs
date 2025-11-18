using System.Collections.Generic;

namespace ControleEstoque.Api.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;

       
        public bool Status { get; set; } = true;
        
        
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}