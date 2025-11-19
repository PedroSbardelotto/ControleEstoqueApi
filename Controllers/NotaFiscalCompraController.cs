using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using ControleEstoque.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq; // XML
using System.Globalization; // Formatação

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotaFiscalCompraController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotaFiscalCompraController> _logger;

        public NotaFiscalCompraController(AppDbContext context, ILogger<NotaFiscalCompraController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/notafiscalcompra
        [HttpGet]
        public async Task<ActionResult<List<NotaFiscalListDto>>> GetNotasFiscais()
        {
            try
            {
                var notas = await _context.NotasFiscaisCompra
                    .AsNoTracking()
                    .Include(nf => nf.Fornecedor)
                    .OrderByDescending(nf => nf.DataEmissao)
                    .Select(nf => new NotaFiscalListDto
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
                _logger.LogError(ex, "Erro ao buscar lista de NFs.");
                return StatusCode(500, "Erro interno.");
            }
        }

        // GET: api/notafiscalcompra/{id}
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

        // POST: api/notafiscalcompra (Manual)
        [HttpPost]
        public async Task<ActionResult<NotaFiscalCompra>> CriarNotaFiscalCompra([FromBody] NotaFiscalCompraCreateDto nfDto)
        {
            if (!ModelState.IsValid || !nfDto.Itens.Any())
                return BadRequest("Dados inválidos.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var novaNF = new NotaFiscalCompra
                {
                    FornecedorId = nfDto.FornecedorId,
                    NumeroNota = nfDto.NumeroNota,
                    DataEmissao = nfDto.DataEmissao,
                    ValorTotal = 0 // Será calculado
                };
                await _context.NotasFiscaisCompra.AddAsync(novaNF);
                await _context.SaveChangesAsync();

                decimal total = 0;
                foreach (var itemDto in nfDto.Itens)
                {
                    var produto = await _context.Produtos.FindAsync(itemDto.ProdutoId);
                    if (produto == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"Produto ID {itemDto.ProdutoId} não encontrado.");
                    }

                    produto.Quantidade += itemDto.Quantidade;
                    produto.PrecoCusto = itemDto.PrecoCustoUnitario; // Atualiza custo

                    var itemNF = new NotaFiscalCompraItem
                    {
                        NotaFiscalCompraId = novaNF.Id,
                        ProdutoId = itemDto.ProdutoId,
                        Quantidade = itemDto.Quantidade,
                        PrecoCustoUnitario = itemDto.PrecoCustoUnitario
                    };
                    await _context.NotasFiscaisCompraItens.AddAsync(itemNF);
                    total += (itemDto.Quantidade * itemDto.PrecoCustoUnitario);
                }

                novaNF.ValorTotal = total;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetNotaFiscalCompra), new { id = novaNF.Id }, novaNF);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao criar NF manual.");
                return StatusCode(500, "Erro interno.");
            }
        }

        // POST: api/notafiscalcompra/uploadxml
        [HttpPost("UploadXML")]
        public async Task<ActionResult> UploadXML(IFormFile file) // Mudou de 'arquivo' para 'file' para bater com o JS
        {
            if (file == null || file.Length == 0)
                return BadRequest("Arquivo XML não enviado.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                using var stream = file.OpenReadStream();
                var xml = XDocument.Load(stream);
                XNamespace ns = "http://www.portalfiscal.inf.br/nfe";

                var infNFe = xml.Descendants(ns + "infNFe").FirstOrDefault();
                if (infNFe == null) return BadRequest("XML inválido (tag infNFe não encontrada).");

                // 1. DADOS DA NOTA
                var ide = infNFe.Element(ns + "ide");
                string nNF = ide?.Element(ns + "nNF")?.Value ?? "S/N";
                DateTime dhEmi = DateTime.TryParse(ide?.Element(ns + "dhEmi")?.Value, out var d) ? d : DateTime.Now;

                // 2. FORNECEDOR
                var emit = infNFe.Element(ns + "emit");
                string cnpjEmit = emit?.Element(ns + "CNPJ")?.Value ?? "";
                string nomeEmit = emit?.Element(ns + "xNome")?.Value ?? "Fornecedor Desconhecido";

                // Busca ou Cria Fornecedor
                var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(f => f.Cnpj == cnpjEmit);
                if (fornecedor == null)
                {
                    fornecedor = new Fornecedor { Nome = nomeEmit, Cnpj = cnpjEmit, Email = "xml@import.com" };
                    _context.Fornecedores.Add(fornecedor);
                    await _context.SaveChangesAsync();
                }

                // 3. CABEÇALHO DA NF
                var novaNF = new NotaFiscalCompra
                {
                    FornecedorId = fornecedor.Id,
                    NumeroNota = nNF,
                    DataEmissao = dhEmi,
                    ValorTotal = 0
                };
                _context.NotasFiscaisCompra.Add(novaNF);
                await _context.SaveChangesAsync();

                // 4. ITENS (PRODUTOS)
                decimal valorTotalCalculado = 0;
                var dets = infNFe.Elements(ns + "det");

                foreach (var det in dets)
                {
                    var prod = det.Element(ns + "prod");
                    string xProd = prod?.Element(ns + "xProd")?.Value ?? "Produto XML";
                    string cProd = prod?.Element(ns + "cProd")?.Value ?? ""; // Código do produto no fornecedor

                    // Ler números (Cuidado com ponto/vírgula)
                    decimal qCom = decimal.Parse(prod?.Element(ns + "qCom")?.Value ?? "0", CultureInfo.InvariantCulture);
                    decimal vUnCom = decimal.Parse(prod?.Element(ns + "vUnCom")?.Value ?? "0", CultureInfo.InvariantCulture);

                    // Busca Produto (Pelo nome exato ou código?) 
                    // Vamos simplificar: Busca pelo nome. Se não achar, cria.
                    var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Nome == xProd);

                    if (produto == null)
                    {
                        produto = new Produto
                        {
                            Nome = xProd,
                            Tipo = "Importado",
                            Quantidade = 0,
                            PrecoCusto = vUnCom,
                            PrecoVenda = vUnCom * 1.5m // Margem fictícia de 50%
                        };
                        _context.Produtos.Add(produto);
                        await _context.SaveChangesAsync();
                    }

                    // Atualiza Estoque e Custo
                    produto.Quantidade += (int)qCom;
                    produto.PrecoCusto = vUnCom;

                    // Cria Item da NF
                    var itemNF = new NotaFiscalCompraItem
                    {
                        NotaFiscalCompraId = novaNF.Id,
                        ProdutoId = produto.Id,
                        Quantidade = (int)qCom,
                        PrecoCustoUnitario = vUnCom
                    };
                    _context.NotasFiscaisCompraItens.Add(itemNF);
                    valorTotalCalculado += (qCom * vUnCom);
                }

                // Atualiza Total Final
                novaNF.ValorTotal = valorTotalCalculado;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Retorno para o Frontend (JSON com resumo)
                return Ok(new
                {
                    mensagem = "Importação concluída!",
                    numeroNota = novaNF.NumeroNota,
                    fornecedor = fornecedor.Nome,
                    valorTotal = novaNF.ValorTotal,
                    totalItens = dets.Count()
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro no UploadXML");
                return StatusCode(500, "Erro ao processar XML: " + ex.Message);
            }
        }
    }
}