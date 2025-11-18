using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using ControleEstoque.Api.DTOs; // Importa os novos DTOs
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
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PedidosController> _logger;

        // Injeta o Logger
        public PedidosController(AppDbContext context, ILogger<PedidosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/pedidos (Agora aceita ?status=Pendente, etc.)
        [HttpGet]
        public async Task<ActionResult<List<PedidoListDto>>> GetPedidos([FromQuery] string? status)
        {
            try
            {
                // 1. Começa a query (IQueryable)
                var query = _context.Pedidos.AsNoTracking();

                // 2. Aplica o filtro de status, se ele foi fornecido
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(p => p.Status.ToLower() == status.ToLower());
                }

                // 3. Monta a DTO final
                var pedidosDto = await query
                  .OrderByDescending(p => p.DataPedido) // Ordena (mais novos primeiro)
                                    .Select(p => new PedidoListDto
                                    {
                                        Id = p.Id,
                                        DataPedido = p.DataPedido,
                                        NomeCliente = p.Cliente != null ? p.Cliente.Nome : "Cliente Excluído",
                                        ValorTotal = p.PedidoItens.Sum(item => item.Quantidade * item.PrecoUnitarioVenda),
                                        Status = p.Status // <-- LINHA ADICIONADA
                                    })
                  .ToListAsync();

                return Ok(pedidosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a lista de pedidos.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // GET: api/pedidos/{id}
        [HttpGet("{id}", Name = "GetPedido")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            try
            {
                var pedido = await _context.Pedidos
                                    .Include(p => p.Cliente)
                                    .Include(p => p.PedidoItens)
                                        .ThenInclude(pi => pi.Produto)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.Id == id);

                if (pedido == null)
                {
                    return NotFound($"Pedido com ID {id} não encontrado.");
                }
                return Ok(pedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar o pedido com ID {id}.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // POST: api/pedidos (Lógica de Múltiplos Itens)
        [HttpPost]
        public async Task<ActionResult<Pedido>> CriarPedido([FromBody] PedidoCreateDto pedidoDto)
        {
            // Valida o DTO (ClienteId e se a lista de Itens não está vazia)
            if (!ModelState.IsValid || !pedidoDto.Itens.Any())
            {
                return BadRequest("Dados do pedido inválidos ou nenhum item fornecido.");
            }

            // Inicia uma Transação de Banco de Dados
            // Isso garante que ou TUDO funciona (pedido + baixa de estoque), ou NADA é salvo.
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Criar o Cabeçalho do Pedido
                var novoPedido = new Pedido
                {
                    ClienteId = pedidoDto.ClienteId,
                    DataPedido = DateTime.UtcNow, // Define a data atual
                    Status = "Pendente"
                };
                await _context.Pedidos.AddAsync(novoPedido);
                // Salva o cabeçalho primeiro para obter o 'novoPedido.Id'
                await _context.SaveChangesAsync();

                var listaPedidoItens = new List<PedidoItem>();

                // 2. Processar cada Item do "Carrinho"
                foreach (var itemDto in pedidoDto.Itens)
                {
                    // 2a. Buscar o produto no banco
                    var produtoEstoque = await _context.Produtos.FindAsync(itemDto.ProdutoId);

                    // 2b. Validar (Produto existe? Tem estoque?)
                    if (produtoEstoque == null)
                    {
                        await transaction.RollbackAsync(); // Desfaz o pedido
                        return BadRequest($"Produto com ID {itemDto.ProdutoId} não encontrado.");
                    }
                    if (produtoEstoque.Quantidade < itemDto.Quantidade)
                    {
                        await transaction.RollbackAsync(); // Desfaz o pedido
                        return BadRequest($"Estoque insuficiente para o produto '{produtoEstoque.Nome}'. Disponível: {produtoEstoque.Quantidade}.");
                    }

                    // 2c. Deduzir do estoque
                    produtoEstoque.Quantidade -= itemDto.Quantidade;
                    _context.Entry(produtoEstoque).State = EntityState.Modified;

                    // 2d. Criar o PedidoItem
                    var novoPedidoItem = new PedidoItem
                    {
                        PedidoId = novoPedido.Id, // Associa ao cabeçalho que acabamos de criar
                        ProdutoId = itemDto.ProdutoId,
                        Quantidade = itemDto.Quantidade,
                        PrecoUnitarioVenda = produtoEstoque.PrecoVenda // "Congela" o preço de venda
                    };
                    listaPedidoItens.Add(novoPedidoItem);
                }

                // 3. Adicionar todos os itens ao contexto
                await _context.PedidoItens.AddRangeAsync(listaPedidoItens);

                // 4. Salvar todas as mudanças (baixa de estoque E criação dos PedidoItens)
                await _context.SaveChangesAsync();

                // 5. Se tudo deu certo, "Commita" a transação
                await transaction.CommitAsync();

                // 6. Retorna o pedido completo
                var pedidoCriado = await _context.Pedidos
                                         .Include(p => p.Cliente)
                                         .Include(p => p.PedidoItens)
                                             .ThenInclude(pi => pi.Produto)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(p => p.Id == novoPedido.Id);

                return CreatedAtAction(nameof(GetPedido), new { id = novoPedido.Id }, pedidoCriado);
            }
            catch (Exception ex)
            {
                // 5b. Se qualquer coisa falhar (ex: erro de banco), "Reverte" a transação
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro inesperado ao criar pedido.");
                return StatusCode(500, "Erro interno ao processar o pedido.");
            }
        }

        // PUT: api/pedidos/{id}/status (Ex: Mudar para "Concluído")
        [HttpPut("{id}/status")]
        public async Task<IActionResult> AtualizarStatusPedido(int id, [FromBody] string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest("O status não pode ser nulo ou vazio.");
            }

            try
            {
                var pedido = await _context.Pedidos.FindAsync(id);
                if (pedido == null)
                {
                    return NotFound($"Pedido com ID {id} não encontrado.");
                }

                pedido.Status = status;
                _context.Entry(pedido).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar status do pedido ID {id}.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // DELETE: api/pedidos/{id} (Devolve estoque ao cancelar)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarPedido(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pedido = await _context.Pedidos
                                     .Include(p => p.PedidoItens) // PRECISA incluir os itens
                                     .FirstOrDefaultAsync(p => p.Id == id);

                if (pedido == null)
                {
                    return NotFound($"Pedido com ID {id} não encontrado.");
                }

                // Lógica de Negócio: Devolver os itens ao estoque
                foreach (var item in pedido.PedidoItens)
                {
                    var produtoEstoque = await _context.Produtos.FindAsync(item.ProdutoId);
                    if (produtoEstoque != null)
                    {
                        produtoEstoque.Quantidade += item.Quantidade;
                        _context.Entry(produtoEstoque).State = EntityState.Modified;
                    }
                }

                // Remove os PedidoItens
                _context.PedidoItens.RemoveRange(pedido.PedidoItens);
                // Remove o Pedido (cabeçalho)
                _context.Pedidos.Remove(pedido);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao deletar o pedido ID {id}.");
                return StatusCode(500, "Erro interno ao deletar o pedido.");
            }
        }
    }
}