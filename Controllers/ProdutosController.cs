using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // 1. ADICIONADO para Logging

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProdutosController> _logger; // 2. CAMPO PARA O LOGGER

        // 3. INJETADO o ILogger no construtor
        public ProdutosController(AppDbContext context, ILogger<ProdutosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/produtos
        [HttpGet]
        public async Task<ActionResult<List<Produto>>> GetProdutos()
        {
            try
            {
                return await _context.Produtos.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a lista de produtos.");
                return StatusCode(500, "Erro interno do servidor ao buscar produtos.");
            }
        }

        // GET: api/produtos/{id}
        [HttpGet("{id}", Name = "GetProduto")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            try
            {
                var produto = await _context.Produtos.FindAsync(id);

                if (produto == null)
                {
                    return NotFound($"Produto com ID {id} não encontrado.");
                }

                return produto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar o produto com ID {id}.");
                return StatusCode(500, "Erro interno do servidor ao buscar produto.");
            }
        }

        // POST: api/produtos
        [HttpPost]
        public async Task<ActionResult<Produto>> CriarProduto([FromBody] Produto novoProduto)
        {
            // O [FromBody] agora espera um JSON com 'precoVenda' e 'precoCusto'
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _context.Produtos.AddAsync(novoProduto);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduto), new { id = novoProduto.Id }, novoProduto);
            }
            catch (DbUpdateException ex) // Erro específico ao salvar no banco
            {
                _logger.LogError(ex, "Erro ao salvar novo produto no banco.");
                return StatusCode(500, "Erro ao salvar produto. Verifique os dados.");
            }
            catch (Exception ex) // Erro genérico
            {
                _logger.LogError(ex, "Erro inesperado ao criar produto.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // PUT: api/produtos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarProduto(int id, [FromBody] Produto produtoAtualizado)
        {
            // O [FromBody] agora espera 'precoVenda' e 'precoCusto'
            if (id != produtoAtualizado.Id)
            {
                return BadRequest("O ID da URL não corresponde ao ID do produto fornecido.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(produtoAtualizado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) // Erro específico de concorrência ou ID não existente
            {
                if (!_context.Produtos.Any(e => e.Id == id))
                {
                    _logger.LogWarning($"Tentativa de atualizar produto com ID {id} que não foi encontrado.");
                    return NotFound($"Produto com ID {id} não encontrado para atualização.");
                }
                else
                {
                    _logger.LogError(ex, $"Erro de concorrência ao atualizar produto ID {id}.");
                    throw; // Re-lança a exceção se for um erro de concorrência real
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado ao atualizar produto ID {id}.");
                return StatusCode(500, "Erro interno do servidor.");
            }

            return NoContent();
        }

        // DELETE: api/produtos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarProduto(int id)
        {
            try
            {
                var produto = await _context.Produtos.FindAsync(id);
                if (produto == null)
                {
                    return NotFound($"Produto com ID {id} não encontrado para exclusão.");
                }

                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Erro ao deletar produto ID {id} (provável conflito de chave estrangeira com Pedidos).");
                return StatusCode(500, "Erro ao deletar produto. Ele pode estar associado a pedidos existentes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado ao deletar produto ID {id}.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    } // <- A chave '}' faltante estava provavelmente aqui
} // <- Ou aqui