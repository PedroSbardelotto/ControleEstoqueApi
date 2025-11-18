using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using ControleEstoque.Api.DTOs; // Necessário para os DTOs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necessário para .Include e .Select
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq; // Necessário para .Select e .OrderBy
using System.Threading.Tasks;

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Correto (sem Roles)
    public class NotaFiscalCompraController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotaFiscalCompraController> _logger;

        public NotaFiscalCompraController(AppDbContext context, ILogger<NotaFiscalCompraController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/notafiscalcompra
        [HttpPost]
        public async Task<ActionResult<NotaFiscalCompra>> CriarNotaFiscalCompra([FromBody] NotaFiscalCompraCreateDto nfDto)
        {
            if (!ModelState.IsValid || !nfDto.Itens.Any())
            {
                return BadRequest("Dados da NF inválidos ou nenhum item fornecido.");
            }

            // Inicia a Transação
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Criar o Cabeçalho da NF
                var novaNF = new NotaFiscalCompra
                {
                    FornecedorId = nfDto.FornecedorId,
                    NumeroNota = nfDto.NumeroNota,
                    DataEmissao = nfDto.DataEmissao,
                    ValorTotal = nfDto.Itens.Sum(i => i.PrecoCustoUnitario * i.Quantidade)
                };
                await _context.NotasFiscaisCompra.AddAsync(novaNF);
                // Salva para obter o ID da NF
                await _context.SaveChangesAsync();

                var listaItensNF = new List<NotaFiscalCompraItem>();

                // 2. Processar cada item da NF
                foreach (var itemDto in nfDto.Itens)
                {
                    // 2a. Achar o produto no banco
                    var produtoEstoque = await _context.Produtos.FindAsync(itemDto.ProdutoId);

                    // 2b. VALIDAÇÃO
                    if (produtoEstoque == null)
                    {
                        await transaction.RollbackAsync(); // Desfaz a criação da NF
                        return BadRequest($"Produto com ID {itemDto.ProdutoId} não está cadastrado. Cadastre-o antes.");
                    }

                    // 2c. ATUALIZA O ESTOQUE (Soma a quantidade)
                    produtoEstoque.Quantidade += itemDto.Quantidade;

                    // Opcional: Atualiza o Preço de Custo no cadastro do produto
                    produtoEstoque.PrecoCusto = itemDto.PrecoCustoUnitario;

                    _context.Entry(produtoEstoque).State = EntityState.Modified;

                    // 2d. Criar a linha/item da NF
                    var novoItemNF = new NotaFiscalCompraItem
                    {
                        NotaFiscalCompraId = novaNF.Id, // Linka com o cabeçalho
                        ProdutoId = itemDto.ProdutoId,
                        Quantidade = itemDto.Quantidade,
                        PrecoCustoUnitario = itemDto.PrecoCustoUnitario
                    };
                    listaItensNF.Add(novoItemNF);
                }

                // 3. Adicionar todos os itens da NF ao contexto
                await _context.NotasFiscaisCompraItens.AddRangeAsync(listaItensNF);

                // 4. Salvar as mudanças
                await _context.SaveChangesAsync();

                // 5. Commita a transação
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetNotaFiscalCompra), new { id = novaNF.Id }, novaNF);
            }
            catch (Exception ex)
            {
                // 5b. Se algo falhar, reverte tudo
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro inesperado ao criar Nota Fiscal de Compra.");
                return StatusCode(500, "Erro interno ao processar a Nota Fiscal.");
            }
        }


        // --- (BLOCO DE CÓDIGO ADICIONADO - Backlog Item 2.1) ---
        // GET: api/notafiscalcompra 
        [HttpGet]
        public async Task<ActionResult<List<NotaFiscalListDto>>> GetNotasFiscais()
        {
            try
            {
                var notas = await _context.NotasFiscaisCompra
                    .AsNoTracking()
                    .Include(nf => nf.Fornecedor) // Inclui o Fornecedor para pegar o Nome
                    .OrderByDescending(nf => nf.DataEmissao)
                    .Select(nf => new NotaFiscalListDto // Usa o DTO de Lista
                    {
                        Id = nf.Id,
                        NumeroNota = nf.NumeroNota,
                        NomeFornecedor = nf.Fornecedor.Nome,
                        DataEmissao = nf.DataEmissao,
                        ValorTotal = nf.ValorTotal
                    })
                    .ToListAsync();

                return Ok(notas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a lista de notas fiscais.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
        // --- FIM DO BLOCO ADICIONADO ---


        // GET (só para o CreatedAtAction funcionar)
        [HttpGet("{id}", Name = "GetNotaFiscalCompra")]
        public async Task<ActionResult<NotaFiscalCompra>> GetNotaFiscalCompra(int id)
        {
            var nf = await _context.NotasFiscaisCompra
                .Include(n => n.Fornecedor)
                .Include(n => n.Itens)
                    .ThenInclude(i => i.Produto)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nf == null) return NotFound();
            return Ok(nf);
        }
    }
}