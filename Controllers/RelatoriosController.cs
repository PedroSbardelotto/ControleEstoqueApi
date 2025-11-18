using ControleEstoque.Api.Data;
using ControleEstoque.Api.DTOs; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Necessário para Sum
using System.Threading.Tasks;
using System; // Adicionado para DateTime
using System.Collections.Generic; // Adicionado para List

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // <-- CORREÇÃO 1: Removida a Role "admin"
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

           

            // 1. Calcula o valor total de CUSTO (Custo * Qtd)
            
            decimal valorTotalCusto = await _context.Produtos
                                         .SumAsync(p => (decimal?)(p.PrecoCusto * p.Quantidade)) ?? 0;

            // 2. Calcula o valor total de VENDA (Venda * Qtd)
           
            decimal valorTotalVenda = await _context.Produtos
                                         .SumAsync(p => (decimal?)(p.PrecoVenda * p.Quantidade)) ?? 0;

           

            // Cria o objeto de resposta (DTO)
            var resultado = new VisaoGeralEstoqueDto
            {
                TotalItensUnicos = totalItensUnicos,
                ValorTotalEstoqueCusto = valorTotalCusto, // Novo campo
                ValorTotalEstoqueVenda = valorTotalVenda  // Campo renomeado
            };

            // Retorna os dados com status 200 OK
            return Ok(resultado);
        }

        // --- INÍCIO DOS NOVOS ENDPOINTS ---

        // 1. GET: api/relatorios/vendas/top5produtos
        [HttpGet("vendas/top5produtos")]
        public async Task<ActionResult> GetTop5ProdutosMaisVendidos()
        {
            var topProdutos = await _context.PedidoItens
                .GroupBy(pi => new { pi.ProdutoId, pi.Produto.Nome }) // Agrupa por ID e Nome do Produto
                .Select(g => new {
                    //ProdutoId = g.Key.ProdutoId,
                    NomeProduto = g.Key.Nome,
                    TotalVendido = g.Sum(pi => pi.Quantidade) // Soma as quantidades vendidas
                })
                .OrderByDescending(x => x.TotalVendido) // Ordena pela maior quantidade
                .Take(5) // Pega os 5 primeiros
                .ToListAsync();

            return Ok(topProdutos);
        }

        // 2. GET: api/relatorios/vendas/top5clientes
        [HttpGet("vendas/top5clientes")]
        public async Task<ActionResult> GetTop5Clientes()
        {
            // Esta query assume que "mais compram" = maior valor (R$) total
            var topClientes = await _context.PedidoItens
                .GroupBy(pi => new { pi.Pedido.ClienteId, pi.Pedido.Cliente.Nome }) // Agrupa por Cliente
                .Select(g => new {
                    //ClienteId = g.Key.ClienteId,
                    NomeCliente = g.Key.Nome,
                    ValorTotalComprado = g.Sum(pi => pi.PrecoUnitarioVenda * pi.Quantidade) // Soma o valor
                })
                .OrderByDescending(x => x.ValorTotalComprado) // Ordena pelo maior valor
                .Take(5) // Pega os 5 primeiros
                .ToListAsync();

            return Ok(topClientes);
        }

        // 3. GET: api/relatorios/vendas/pordia (Agrupado por Dia)
        [HttpGet("vendas/pordia")]
        public async Task<ActionResult> GetVendasAgrupadasPorDia()
        {
            var vendas = await _context.PedidoItens
                .GroupBy(pi => pi.Pedido.DataPedido.Date) // Agrupa pelo Dia
                .Select(g => new {
                    Data = g.Key.ToString("yyyy-MM-dd"),
                    ValorTotalVendido = g.Sum(pi => pi.PrecoUnitarioVenda * pi.Quantidade),
                    TotalPedidos = g.Select(pi => pi.PedidoId).Distinct().Count() // Conta pedidos únicos
                })
                .OrderByDescending(x => x.Data)
                .ToListAsync();

            return Ok(vendas);
        }

        // 3b. GET: api/relatorios/vendas/pormes (Agrupado por Mês/Ano)
        [HttpGet("vendas/pormes")]
        public async Task<ActionResult> GetVendasAgrupadasPorMes()
        {
            var vendas = await _context.PedidoItens
                .GroupBy(pi => new { pi.Pedido.DataPedido.Year, pi.Pedido.DataPedido.Month }) // Agrupa por Mês e Ano
                .Select(g => new {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    ValorTotalVendido = g.Sum(pi => pi.PrecoUnitarioVenda * pi.Quantidade),
                    TotalPedidos = g.Select(pi => pi.PedidoId).Distinct().Count()
                })
                .OrderByDescending(x => x.Ano).ThenByDescending(x => x.Mes)
                .ToListAsync();

            return Ok(vendas);
        }

        // 4. GET: api/relatorios/estoque/diascomsaida
        [HttpGet("estoque/diascomsaida")]
        public async Task<ActionResult> GetDiasComSaidaDeEstoque()
        {
            // Simplesmente lista os dias únicos em que pedidos foram feitos
            var diasComSaida = await _context.Pedidos
                .Select(p => p.DataPedido.Date) // Pega a Data (ignora a hora)
                .Distinct() // Apenas valores únicos
                .OrderByDescending(date => date)
                .ToListAsync();

            return Ok(diasComSaida);
        }

        // 5. GET: api/relatorios/estoque/itensparados
        [HttpGet("estoque/itensparados")]
        public async Task<ActionResult> GetItensParados([FromQuery] int dias = 10)
        {
            // Data limite (Ex: 10 dias atrás a partir de agora)
            var dataLimite = DateTime.UtcNow.AddDays(-dias);

            var itensParados = await _context.Produtos
                // 1. Filtra apenas produtos que têm estoque (não adianta mostrar zerados)
                .Where(p => p.Quantidade > 0)
                // 2. Transforma (Select) cada produto em um objeto com a data da última venda
                .Select(p => new {
                    Produto = p,
                    // Se (Any) houver itens de pedido, pega o Max (mais recente) da DataPedido.
                    // Se não houver, a última venda é nula (null).
                    UltimaVenda = p.PedidoItens.Any()
                                  ? p.PedidoItens.Max(pi => pi.Pedido.DataPedido)
                                  : (DateTime?)null
                })
                // 3. Filtra:
                //    a) Onde a UltimaVenda for nula (nunca foi vendido)
                //    OU
                //    b) Onde a UltimaVenda foi ANTES da data limite (ex: 11+ dias atrás)
                .Where(x => x.UltimaVenda == null || x.UltimaVenda < dataLimite)
                // 4. Formata o resultado
                .Select(x => new {
                    x.Produto.Id,
                    x.Produto.Nome,
                    x.Produto.Quantidade,
                    DataUltimaVenda = x.UltimaVenda // Mostra a data (ou null se nunca vendeu)
                })
                .ToListAsync();

            return Ok(itensParados);
        }

        // --- FIM DOS NOVOS ENDPOINTS ---
    }
}