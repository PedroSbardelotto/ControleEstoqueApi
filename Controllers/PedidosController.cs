using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Api.Controllers
{
    [ApiController] 
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/pedidos
        [HttpGet]
        public async Task<ActionResult<List<Pedido>>> GetPedidos()
        {
            return await _context.Pedidos.ToListAsync();
        }

        // GET: api/pedidos/{id}
        [HttpGet("{id}", Name = "GetPedido")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado.");
            }

            return pedido;
        }

        // POST: api/pedidos
        [HttpPost]
        public async Task<ActionResult<Pedido>> CriarPedido([FromBody] Pedido novoPedido)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _context.Pedidos.AddAsync(novoPedido);
            await _context.SaveChangesAsync();

            // Retorna 201 Created com a localização do novo recurso e o objeto criado
            // Certifique-se que GetPedido aceita int como parâmetro
            return CreatedAtAction(nameof(GetPedido), new { id = novoPedido.Id }, novoPedido); // <-- LINHA QUE FALTAVA
        }

        // PUT: api/pedidos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarPedido(int id, [FromBody] Pedido pedidoAtualizado)
        {
            if (id != pedidoAtualizado.Id)
            {
                return BadRequest("O ID da URL não corresponde ao ID do pedido fornecido.");
            }

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
            } // Fechamento do catch

            return NoContent();
        } // Fechamento do AtualizarPedido

        // DELETE: api/pedidos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarPedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado para exclusão.");
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    } 
} 