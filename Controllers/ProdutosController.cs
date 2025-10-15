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
    public class ProdutosController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public ProdutosController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Produto>>> GetProdutos() =>
            await _context.Produtos.Find(_ => true).ToListAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Produto>> GetProduto(string id)
        {
            var produto = await _context.Produtos.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (produto is null)
            {
                return NotFound();
            }
            return produto;
        }

        [HttpPost]
        public async Task<IActionResult> CriarProduto(Produto novoProduto)
        {
            await _context.Produtos.InsertOneAsync(novoProduto);
            return CreatedAtAction(nameof(GetProduto), new { id = novoProduto.Id }, novoProduto);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> AtualizarProduto(string id, Produto produtoAtualizado)
        {
            var produto = await _context.Produtos.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (produto is null)
            {
                return NotFound();
            }

            produtoAtualizado.Id = produto.Id; // Garante que o ID não seja alterado
            await _context.Produtos.ReplaceOneAsync(p => p.Id == id, produtoAtualizado);

            return NoContent(); // Resposta padrão para um update bem-sucedido
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeletarProduto(string id)
        {
            var produto = await _context.Produtos.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (produto is null)
            {
                return NotFound();
            }

            await _context.Produtos.DeleteOneAsync(p => p.Id == id);

            return NoContent(); // Resposta padrão para um delete bem-sucedido
        }
    }
}