using ControleEstoque.Api.Data;
using ControleEstoque.Api.DTOs;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Necessário para Any() e Include()
using System.Threading.Tasks;

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Injeta o AppDbContext
        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/pedidos
        [HttpGet]
        public async Task<ActionResult<List<Pedido>>> GetPedidos()
        {
            // Retorna a lista de pedidos. Considerar incluir dados do Cliente/Produto se necessário.
            // Ex: return await _context.Pedidos.Include(p => p.Cliente).Include(p => p.Produto).ToListAsync();
            return await _context.Pedidos.ToListAsync();
        }

        // GET: api/pedidos/{id}
        [HttpGet("{id}", Name = "GetPedido")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            // Busca o pedido E inclui os dados do Cliente e Produto relacionados
            var pedido = await _context.Pedidos
                                       .Include(p => p.Cliente) // Inclui dados do Cliente
                                       .Include(p => p.Produto) // Inclui dados do Produto
                                       .FirstOrDefaultAsync(p => p.Id == id); // Busca pelo ID

            if (pedido == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado.");
            }

            // Retorna o pedido com os detalhes do cliente e produto
            return pedido;
        }

        // POST: api/pedidos
        [HttpPost]
        // Alterado para receber o DTO
        public async Task<ActionResult<Pedido>> CriarPedido([FromBody] PedidoCreateDto pedidoDto)
        {
            // A validação agora usa os atributos do DTO
            if (!ModelState.IsValid)
            {
                // Retorna os erros detalhados do DTO
                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Title = "Um ou mais erros de validação ocorreram no DTO.",
                    Detail = "Verifique os erros no campo 'errors'."
                });
            }

            // --- VERIFICAÇÃO DE ESTOQUE ---
            var produtoEstoque = await _context.Produtos.FindAsync(pedidoDto.ProdutoId);

            if (produtoEstoque == null)
            {
                ModelState.AddModelError(nameof(pedidoDto.ProdutoId), $"Produto com ID {pedidoDto.ProdutoId} não encontrado.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (produtoEstoque.Quantidade < pedidoDto.QuantidadeProduto)
            {
                ModelState.AddModelError(nameof(pedidoDto.QuantidadeProduto), $"Estoque insuficiente para o produto '{produtoEstoque.Nome}'. Disponível: {produtoEstoque.Quantidade}.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // --- ATUALIZAÇÃO DO ESTOQUE ---
            produtoEstoque.Quantidade -= pedidoDto.QuantidadeProduto;
            _context.Entry(produtoEstoque).State = EntityState.Modified;

            // --- CRIAÇÃO DO PEDIDO (Mapeando do DTO para a Entidade) ---
            var novoPedido = new Pedido
            {
                ProdutoId = pedidoDto.ProdutoId,
                ClienteId = pedidoDto.ClienteId,
                QuantidadeProduto = pedidoDto.QuantidadeProduto
                // O EF Core cuidará de associar aos objetos Cliente e Produto pelo ID
            };

            await _context.Pedidos.AddAsync(novoPedido);
            await _context.SaveChangesAsync();

            // Busca o pedido recém-criado incluindo Cliente e Produto para retornar ao front-end
            var pedidoCriado = await _context.Pedidos
                                            .Include(p => p.Cliente)
                                            .Include(p => p.Produto)
                                            .FirstOrDefaultAsync(p => p.Id == novoPedido.Id);

            // Retorna 201 Created com o pedido completo (incluindo objetos Cliente/Produto)
            return CreatedAtAction(nameof(GetPedido), new { id = novoPedido.Id }, pedidoCriado);
        }

        // PUT: api/pedidos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarPedido(int id, [FromBody] Pedido pedidoAtualizado)
        {
            if (id != pedidoAtualizado.Id)
            {
                return BadRequest("O ID da URL não corresponde ao ID do pedido fornecido.");
            }

            // ATENÇÃO: Atualizar um pedido PODE exigir lógica complexa para
            // reajustar o estoque (devolver estoque antigo, retirar novo estoque).
            // Por simplicidade, este exemplo apenas atualiza os dados do pedido.
            // Verifique se ClienteId e ProdutoId enviados existem, se necessário.

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(pedidoAtualizado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pedidos.Any(e => e.Id == id))
                {
                    return NotFound($"Pedido com ID {id} não encontrado para atualização.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/pedidos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarPedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado para exclusão.");
            }

            // ATENÇÃO: Ao deletar um pedido, você PODE querer restaurar
            // a quantidade do produto no estoque. Esta lógica não está incluída aqui.
            // Exemplo (requer buscar o produto):
            // var produto = await _context.Produtos.FindAsync(pedido.ProdutoId);
            // if (produto != null) {
            //     produto.Quantidade += pedido.QuantidadeProduto;
            //     _context.Entry(produto).State = EntityState.Modified;
            // }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync(); // Salvaria a remoção do pedido e a atualização do estoque

            return NoContent();
        }
    }
}