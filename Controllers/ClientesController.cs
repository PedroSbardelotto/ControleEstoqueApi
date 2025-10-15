using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly MongoDbContext _context;

        // O "Gerente" (sistema de injeção de dependência) entrega ao atendente
        // um canal de comunicação direto com a cozinha (_context) quando ele é criado.
        public ClientesController(MongoDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<List<Cliente>>> GetClientes()
        {
            return await _context.Clientes.Find(_ => true).ToListAsync();
        }
        // GET: api/clientes/{id}
        [HttpGet("{id:length(24)}", Name = "GetCliente")]
        public async Task<ActionResult<Cliente>> GetCliente(string id)
        {
            var cliente = await _context.Clientes.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (cliente == null)
            {
                return NotFound();
            }
            return cliente;
        }
        // POST: api/clientes
        [HttpPost]  
        public async Task<ActionResult<Cliente>> CreateCliente(Cliente cliente)
        {
            await _context.Clientes.InsertOneAsync(cliente);
            return CreatedAtAction("GetCliente", new { id = cliente.Id }, cliente);
        }
    }
}