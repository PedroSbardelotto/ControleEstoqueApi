using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Apenas Admins podem gerenciar fornecedores
    public class FornecedoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FornecedoresController> _logger;

        public FornecedoresController(AppDbContext context, ILogger<FornecedoresController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/fornecedores
        [HttpGet]
        public async Task<ActionResult<List<Fornecedor>>> GetFornecedores()
        {
            try
            {
                return await _context.Fornecedores.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar fornecedores.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // GET: api/fornecedores/{id}
        [HttpGet("{id}", Name = "GetFornecedor")]
        public async Task<ActionResult<Fornecedor>> GetFornecedor(int id)
        {
            try
            {
                var fornecedor = await _context.Fornecedores.FindAsync(id);
                if (fornecedor == null)
                {
                    return NotFound($"Fornecedor com ID {id} não encontrado.");
                }
                return Ok(fornecedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar fornecedor ID {id}.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // POST: api/fornecedores
        [HttpPost]
        public async Task<ActionResult<Fornecedor>> CriarFornecedor([FromBody] Fornecedor novoFornecedor)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                await _context.Fornecedores.AddAsync(novoFornecedor);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetFornecedor), new { id = novoFornecedor.Id }, novoFornecedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar fornecedor.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // PUT: api/fornecedores/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarFornecedor(int id, [FromBody] Fornecedor fornecedorAtualizado)
        {
            if (id != fornecedorAtualizado.Id)
            {
                return BadRequest("ID da URL não corresponde ao ID do fornecedor.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(fornecedorAtualizado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.Fornecedores.Any(e => e.Id == id))
                {
                    return NotFound($"Fornecedor com ID {id} não encontrado.");
                }
                _logger.LogError(ex, $"Erro de concorrência ao atualizar fornecedor ID {id}.");
                return StatusCode(500, "Erro de concorrência.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar fornecedor ID {id}.");
                return StatusCode(500, "Erro interno do servidor.");
            }
            return NoContent();
        }

        // DELETE: api/fornecedores/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarFornecedor(int id)
        {
            try
            {
                var fornecedor = await _context.Fornecedores.FindAsync(id);
                if (fornecedor == null)
                {
                    return NotFound($"Fornecedor com ID {id} não encontrado.");
                }
                _context.Fornecedores.Remove(fornecedor);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao deletar fornecedor ID {id} (pode estar ligado a uma NF).");
                return StatusCode(500, "Erro interno ao deletar fornecedor.");
            }
        }
    }
}
