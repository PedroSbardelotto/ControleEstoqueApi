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
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(AppDbContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<List<Cliente>>> GetClientes()
        {
            try
            {
                // --- CORREÇÃO SOFT DELETE ---
                // Retorna apenas clientes ATIVOS
                var clientes = await _context.Clientes
                                        .Where(c => c.Status == true)
                                        .ToListAsync();
                // --- FIM DA CORREÇÃO ---

                return Ok(clientes);
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
            // (Este endpoint pode retornar um cliente inativo, o que é útil
            // para visualização de histórico ou reativação)
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);

                if (cliente == null)
                {
                    _logger.LogWarning($"Cliente com ID {id} não encontrado.");
                    return NotFound($"Cliente com ID {id} não encontrado.");
                }

                return Ok(cliente);
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
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // --- CORREÇÃO SOFT DELETE ---
                // Garante que o novo cliente seja sempre criado como Ativo
                novoCliente.Status = true;
                // --- FIM DA CORREÇÃO ---

                await _context.Clientes.AddAsync(novoCliente);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCliente), new { id = novoCliente.Id }, novoCliente);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao salvar novo cliente no banco.");
                return StatusCode(500, "Erro ao salvar cliente. Verifique os dados.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar cliente.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // PUT: api/clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarCliente(int id, [FromBody] Cliente clienteAtualizado)
        {
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

                // --- CORREÇÃO SOFT DELETE (Evita bug de atualização cega) ---

                // 1. Busca o cliente original do banco
                var clienteDoBanco = await _context.Clientes.FindAsync(id);

                if (clienteDoBanco == null)
                {
                    _logger.LogWarning($"Tentativa de atualizar cliente ID {id} que não foi encontrado.");
                    return NotFound($"Cliente com ID {id} não encontrado para atualização.");
                }

                // 2. Copia apenas os dados que podem ser alterados
                clienteDoBanco.Nome = clienteAtualizado.Nome;
                clienteDoBanco.CNPJ = clienteAtualizado.CNPJ;
                clienteDoBanco.Email = clienteAtualizado.Email;
                clienteDoBanco.Endereco = clienteAtualizado.Endereco;

                // O 'clienteDoBanco.Status' (que é 'true') é preservado.
                // O 'clienteAtualizado.Status' (potencialmente 'false') é ignorado.

                // 3. Salva o objeto lido do banco, agora modificado
                await _context.SaveChangesAsync();

                // --- FIM DA CORREÇÃO ---

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.Clientes.Any(e => e.Id == id))
                {
                    // Este bloco pode ser redundante devido à verificação 'FindAsync' acima,
                    // mas mantê-lo é seguro.
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
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                {
                    _logger.LogWarning($"Cliente com ID {id} não encontrado para exclusão.");
                    return NotFound($"Cliente com ID {id} não encontrado para exclusão.");
                }

                // --- CORREÇÃO SOFT DELETE ---
                // Em vez de remover, definimos o status como inativo
                cliente.Status = false;
                // --- FIM DA CORREÇÃO ---

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex) // Este catch agora só será chamado por outros erros de update
            {
                _logger.LogError(ex, $"Erro ao inativar cliente ID {id}.");
                return StatusCode(500, "Erro ao inativar o cliente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado ao inativar cliente ID {id}.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}