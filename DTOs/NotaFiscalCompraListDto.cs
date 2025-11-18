using System;

namespace ControleEstoque.Api.DTOs
{
    // DTO para a listagem de NFs
    public class NotaFiscalListDto
    {
        public int Id { get; set; }
        public string NumeroNota { get; set; }
        public string NomeFornecedor { get; set; }
        public DateTime DataEmissao { get; set; }
        public decimal ValorTotal { get; set; }
    }
}