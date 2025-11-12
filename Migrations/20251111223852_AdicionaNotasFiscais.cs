using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaNotasFiscais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotaFiscalCompra_Fornecedores_FornecedorId",
                table: "NotaFiscalCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_NotaFiscalCompraItem_NotaFiscalCompra_NotaFiscalCompraId",
                table: "NotaFiscalCompraItem");

            migrationBuilder.DropForeignKey(
                name: "FK_NotaFiscalCompraItem_Produtos_ProdutoId",
                table: "NotaFiscalCompraItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotaFiscalCompraItem",
                table: "NotaFiscalCompraItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotaFiscalCompra",
                table: "NotaFiscalCompra");

            migrationBuilder.RenameTable(
                name: "NotaFiscalCompraItem",
                newName: "NotasFiscaisCompraItens");

            migrationBuilder.RenameTable(
                name: "NotaFiscalCompra",
                newName: "NotasFiscaisCompra");

            migrationBuilder.RenameIndex(
                name: "IX_NotaFiscalCompraItem_ProdutoId",
                table: "NotasFiscaisCompraItens",
                newName: "IX_NotasFiscaisCompraItens_ProdutoId");

            migrationBuilder.RenameIndex(
                name: "IX_NotaFiscalCompraItem_NotaFiscalCompraId",
                table: "NotasFiscaisCompraItens",
                newName: "IX_NotasFiscaisCompraItens_NotaFiscalCompraId");

            migrationBuilder.RenameIndex(
                name: "IX_NotaFiscalCompra_FornecedorId",
                table: "NotasFiscaisCompra",
                newName: "IX_NotasFiscaisCompra_FornecedorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotasFiscaisCompraItens",
                table: "NotasFiscaisCompraItens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotasFiscaisCompra",
                table: "NotasFiscaisCompra",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotasFiscaisCompra_Fornecedores_FornecedorId",
                table: "NotasFiscaisCompra",
                column: "FornecedorId",
                principalTable: "Fornecedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotasFiscaisCompraItens_NotasFiscaisCompra_NotaFiscalCompraId",
                table: "NotasFiscaisCompraItens",
                column: "NotaFiscalCompraId",
                principalTable: "NotasFiscaisCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotasFiscaisCompraItens_Produtos_ProdutoId",
                table: "NotasFiscaisCompraItens",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotasFiscaisCompra_Fornecedores_FornecedorId",
                table: "NotasFiscaisCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_NotasFiscaisCompraItens_NotasFiscaisCompra_NotaFiscalCompraId",
                table: "NotasFiscaisCompraItens");

            migrationBuilder.DropForeignKey(
                name: "FK_NotasFiscaisCompraItens_Produtos_ProdutoId",
                table: "NotasFiscaisCompraItens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotasFiscaisCompraItens",
                table: "NotasFiscaisCompraItens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotasFiscaisCompra",
                table: "NotasFiscaisCompra");

            migrationBuilder.RenameTable(
                name: "NotasFiscaisCompraItens",
                newName: "NotaFiscalCompraItem");

            migrationBuilder.RenameTable(
                name: "NotasFiscaisCompra",
                newName: "NotaFiscalCompra");

            migrationBuilder.RenameIndex(
                name: "IX_NotasFiscaisCompraItens_ProdutoId",
                table: "NotaFiscalCompraItem",
                newName: "IX_NotaFiscalCompraItem_ProdutoId");

            migrationBuilder.RenameIndex(
                name: "IX_NotasFiscaisCompraItens_NotaFiscalCompraId",
                table: "NotaFiscalCompraItem",
                newName: "IX_NotaFiscalCompraItem_NotaFiscalCompraId");

            migrationBuilder.RenameIndex(
                name: "IX_NotasFiscaisCompra_FornecedorId",
                table: "NotaFiscalCompra",
                newName: "IX_NotaFiscalCompra_FornecedorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotaFiscalCompraItem",
                table: "NotaFiscalCompraItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotaFiscalCompra",
                table: "NotaFiscalCompra",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotaFiscalCompra_Fornecedores_FornecedorId",
                table: "NotaFiscalCompra",
                column: "FornecedorId",
                principalTable: "Fornecedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotaFiscalCompraItem_NotaFiscalCompra_NotaFiscalCompraId",
                table: "NotaFiscalCompraItem",
                column: "NotaFiscalCompraId",
                principalTable: "NotaFiscalCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotaFiscalCompraItem_Produtos_ProdutoId",
                table: "NotaFiscalCompraItem",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
