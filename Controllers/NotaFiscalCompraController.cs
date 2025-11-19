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
using System.Xml.Linq;
using System.IO;

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

        // POST: api/notafiscalcompra/uploadxml
        [HttpPost("UploadXML")]
        public async Task<ActionResult> UploadXML(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest("Arquivo XML não foi enviado.");

            if (!arquivo.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                return BadRequest("O arquivo deve ser um XML válido.");

            // Inicia a Transação (reutilizando a lógica transacional)
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Ler o conteúdo do arquivo XML
                XDocument xmlDoc;
                using (var stream = arquivo.OpenReadStream())
                {
                    xmlDoc = await XDocument.LoadAsync(stream, LoadOptions.None, default);
                }

                // 2. Extrair dados do XML (NFe padrão brasileiro)
                var nfeProc = xmlDoc.Root;

                // Namespace padrão da NFe
                XNamespace nfe = "http://www.portalfiscal.inf.br/nfe";

                // Navega até a tag <infNFe>
                var infNFe = nfeProc?.Descendants(nfe + "infNFe").FirstOrDefault();

                if (infNFe == null)
                    return BadRequest("XML inválido: estrutura de NFe não encontrada.");

                // 3. Extrair dados do Cabeçalho da NF
                var ide = infNFe.Element(nfe + "ide");
                var emit = infNFe.Element(nfe + "emit");
                var total = infNFe.Element(nfe + "total")?.Element(nfe + "ICMSTot");

                string numeroNota = ide?.Element(nfe + "nNF")?.Value;
                string cnpjFornecedor = emit?.Element(nfe + "CNPJ")?.Value;
                string nomeFornecedor = emit?.Element(nfe + "xNome")?.Value;

                DateTime dataEmissao = DateTime.TryParse(
                    ide?.Element(nfe + "dhEmi")?.Value,
                    out var dtEmissao
                ) ? dtEmissao : DateTime.Now;

                decimal valorTotal = decimal.TryParse(
                    total?.Element(nfe + "vNF")?.Value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var vTotal
                ) ? vTotal : 0;

                // 4. Buscar o fornecedor
                var fornecedor = await _context.Fornecedores
                    .FirstOrDefaultAsync(f => f.Cnpj == cnpjFornecedor);

                if (fornecedor == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest($"Fornecedor com CNPJ {cnpjFornecedor} não está cadastrado. Cadastre-o antes de importar a NF.");
                }

                // 5. Extrair os itens do XML
                var detalhes = infNFe.Elements(nfe + "det");
                var listaItensNF = new List<NotaFiscalCompraItem>();
                var valorTotalCalculado = 0m;

                // 6. Criar o Cabeçalho da NF
                var novaNF = new NotaFiscalCompra
                {
                    FornecedorId = fornecedor.Id,
                    NumeroNota = numeroNota,
                    DataEmissao = dataEmissao,
                    ValorTotal = valorTotal // Será atualizado depois
                };
                await _context.NotasFiscaisCompra.AddAsync(novaNF);
                await _context.SaveChangesAsync(); // Salva para obter o ID

                foreach (var det in detalhes)
                {
                    var prod = det.Element(nfe + "prod");

                    string codigoProduto = prod?.Element(nfe + "cProd")?.Value;
                    string nomeProduto = prod?.Element(nfe + "xProd")?.Value;

                    int quantidade = int.TryParse(
                        prod?.Element(nfe + "qCom")?.Value,
                        out var qtd
                    ) ? qtd : 0;

                    decimal precoCusto = decimal.TryParse(
                        prod?.Element(nfe + "vUnCom")?.Value,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var preco
                    ) ? preco : 0;

                    // 7. Tentar localizar o produto pelo nome ou código
                    var produto = await _context.Produtos
                        .FirstOrDefaultAsync(p =>
                            p.Nome.ToLower().Contains(nomeProduto.ToLower()) ||
                            p.Tipo == codigoProduto
                        );

                    if (produto == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"Produto '{nomeProduto}' (código: {codigoProduto}) não encontrado no sistema. Cadastre-o antes.");
                    }

                    // 8. ATUALIZA O ESTOQUE
                    produto.Quantidade += quantidade;
                    produto.PrecoCusto = precoCusto;
                    _context.Entry(produto).State = EntityState.Modified;

                    // 9. Criar a linha/item da NF
                    var novoItemNF = new NotaFiscalCompraItem
                    {
                        NotaFiscalCompraId = novaNF.Id,
                        ProdutoId = produto.Id,
                        Quantidade = quantidade,
                        PrecoCustoUnitario = precoCusto
                    };
                    listaItensNF.Add(novoItemNF);
                    valorTotalCalculado += precoCusto * quantidade;
                }

                // 10. Atualizar valor total (se necessário)
                if (novaNF.ValorTotal == 0)
                {
                    novaNF.ValorTotal = valorTotalCalculado;
                }

                // 11. Adicionar todos os itens
                await _context.NotasFiscaisCompraItens.AddRangeAsync(listaItensNF);
                await _context.SaveChangesAsync();

                // 12. Commita a transação
                await transaction.CommitAsync();

                // 13. Retornar sucesso
                return Ok(new
                {
                    mensagem = "XML importado com sucesso!",
                    numeroNota = numeroNota,
                    fornecedor = nomeFornecedor,
                    valorTotal = novaNF.ValorTotal,
                    totalItens = listaItensNF.Count,
                    notaFiscalId = novaNF.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao processar o arquivo XML da NFe.");
                return StatusCode(500, $"Erro ao processar XML: {ex.Message}");
            }
        }
    }
}