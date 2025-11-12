using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Api.DTOs
{
    public class NotaFiscalCompraCreateDto
    {
        [Required]
        public int FornecedorId { get; set; }

        [Required]
        public string NumeroNota { get; set; }

        public DateTime DataEmissao { get; set; }

        [Required]
        public List<NotaFiscalItemCreateDto> Itens { get; set; } = new List<NotaFiscalItemCreateDto>();
    }
}