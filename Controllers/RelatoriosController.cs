using ControleEstoque.Api.Data;
using ControleEstoque.Api.DTOs; // Adiciona o using para nosso DTO
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Necessário para Sum
using System.Threading.Tasks;

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protege todos os endpoints de relatório
    public class RelatoriosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RelatoriosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/relatorios/estoque/visaogeral
        [HttpGet("estoque/visaogeral")]
        public async Task<ActionResult<VisaoGeralEstoqueDto>> GetVisaoGeralEstoque()
        {
            // Calcula o número total de tipos de produtos diferentes
            int totalItensUnicos = await _context.Produtos.CountAsync();

            // Calcula o valor total do estoque (Preço * Quantidade para cada produto, depois soma tudo)
            decimal valorTotalEstoque = await _context.Produtos
                                             .SumAsync(p => p.Preco * p.Quantidade); // LINQ faz a mágica!

            // Cria o objeto de resposta (DTO)
            var resultado = new VisaoGeralEstoqueDto
            {
                TotalItensUnicos = totalItensUnicos,
                ValorTotalEstoque = valorTotalEstoque
            };

            // Retorna os dados com status 200 OK
            return Ok(resultado);
        }

        // --- Adicionar outros endpoints de relatório aqui no futuro ---

    }
}