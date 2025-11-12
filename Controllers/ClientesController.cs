using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // 1. ADICIONADO (caso estivesse faltando)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClientesController> _logger; // Campo para o Logger

        // 2. CORRIGIDO: Injetando o AppDbContext E o ILogger
        public ClientesController(AppDbContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<List<Cliente>>> GetClientes()
        {
            // 3. ADICIONADO Try/Catch
            try
            {
                var clientes = await _context.Clientes.ToListAsync();
                return Ok(clientes); // Retornar Ok() é uma boa prática
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a lista de clientes.");
                return StatusCode(500, "Erro interno do servidor ao buscar clientes.");
            }
        }

        // GET: api/clientes/{id}
        [HttpGet("{id}", Name = "GetCliente")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            // 4. ADICIONADO Try/Catch
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);

                if (cliente == null)
                {
                    _logger.LogWarning($"Cliente com ID {id} não encontrado.");
                    return NotFound($"Cliente com ID {id} não encontrado.");
                }

                return Ok(cliente); // Retornar Ok()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar o cliente com ID {id}.");
                return StatusCode(500, "Erro interno do servidor ao buscar cliente.");
            }
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> CriarCliente([FromBody] Cliente novoCliente)
        {
            // 5. ADICIONADO Try/Catch
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _context.Clientes.AddAsync(novoCliente);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCliente), new { id = novoCliente.Id }, novoCliente);
            }
            catch (DbUpdateException ex) // Erro específico ao salvar (ex: CNPJ duplicado, se fosse único)
            {
                _logger.LogError(ex, "Erro ao salvar novo cliente no banco.");
                return StatusCode(500, "Erro ao salvar cliente. Verifique os dados.");
            }
            catch (Exception ex) // Erro genérico
            {
                _logger.LogError(ex, "Erro inesperado ao criar cliente.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // PUT: api/clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarCliente(int id, [FromBody] Cliente clienteAtualizado)
        {
            // 6. ADICIONADO Try/Catch (envolvendo a lógica de concorrência)
            try
            {
                if (id != clienteAtualizado.Id)
                {
                    return BadRequest("O ID da URL não corresponde ao ID do corpo da requisição.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _context.Entry(clienteAtualizado).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.Clientes.Any(e => e.Id == id))
                {
                    _logger.LogWarning($"Tentativa de atualizar cliente ID {id} que não foi encontrado.");
                    return NotFound($"Cliente com ID {id} não encontrado para atualização.");
                }
                else
                {
                    _logger.LogError(ex, $"Erro de concorrência ao atualizar cliente ID {id}.");
                    return StatusCode(500, "Erro de concorrência ao salvar dados.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado ao atualizar cliente ID {id}.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // DELETE: api/clientes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarCliente(int id)
        {
            // 7. ADICIONADO Try/Catch
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                {
                    _logger.LogWarning($"Cliente com ID {id} não encontrado para exclusão.");
                    return NotFound($"Cliente com ID {id} não encontrado para exclusão.");
                }

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex) // Captura erros de FK (ex: cliente tem pedidos)
            {
                _logger.LogError(ex, $"Erro ao deletar cliente ID {id} (provável conflito de chave estrangeira com Pedidos).");
                return StatusCode(500, "Erro ao deletar. O cliente pode estar associado a pedidos existentes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado ao deletar cliente ID {id}.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}