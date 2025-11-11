using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstoque.Api.Models
{
    public class NotaFiscalCompra
    {
        public int Id { get; set; }

        // Chave Estrangeira para o Fornecedor
        public int FornecedorId { get; set; }
        public virtual Fornecedor Fornecedor { get; set; } = null!;

        public string NumeroNota { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ValorTotal { get; set; }

        // Uma Nota Fiscal tem uma coleção (lista) de Itens
        public virtual ICollection<NotaFiscalCompraItem> Itens { get; set; } = new List<NotaFiscalCompraItem>();
    }
}