using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public PedidosController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Pedido>>> GetPedidos() =>
            await _context.Pedidos.Find(_ => true).ToListAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Pedido>> GetPedido(string id)
        {
            var pedido = await _context.Pedidos.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (pedido is null)
            {
                return NotFound();
            }

            return pedido;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPedido(Pedido novoPedido)
        {
            await _context.Pedidos.InsertOneAsync(novoPedido);
            return CreatedAtAction(nameof(GetPedido), new { id = novoPedido.Id }, novoPedido);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> AtualizarPedido(string id, Pedido pedidoAtualizado)
        {
            var pedido = await _context.Pedidos.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (pedido is null)
            {
                return NotFound();
            }

            pedidoAtualizado.Id = pedido.Id;
            await _context.Pedidos.ReplaceOneAsync(p => p.Id == id, pedidoAtualizado);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeletarPedido(string id)
        {
            var pedido = await _context.Pedidos.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (pedido is null)
            {
                return NotFound();
            }

            await _context.Pedidos.DeleteOneAsync(p => p.Id == id);

            return NoContent();
        }
    }
}