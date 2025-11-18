using System.Collections.Generic;

namespace ControleEstoque.Api.DTOs
{
    /// <summary>
    /// DTO genérico para alimentar gráficos (ex: Chart.js).
    /// </summary>
    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new List<string>();

        // Usamos decimal para comportar tanto Quantidade (int) quanto Valor (decimal)
        public List<decimal> Valores { get; set; } = new List<decimal>();
    }
}