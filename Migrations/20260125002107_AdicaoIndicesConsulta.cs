using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PraOndeFoi.Migrations
{
    /// <inheritdoc />
    public partial class AdicaoIndicesConsulta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transacoes_ContaId",
                table: "Transacoes");

            migrationBuilder.DropIndex(
                name: "IX_Tags_ContaId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Recorrencias_ContaId",
                table: "Recorrencias");

            migrationBuilder.DropIndex(
                name: "IX_OrcamentosMensais_ContaId",
                table: "OrcamentosMensais");

            migrationBuilder.DropIndex(
                name: "IX_MetasFinanceiras_ContaId",
                table: "MetasFinanceiras");

            migrationBuilder.DropIndex(
                name: "IX_Assinaturas_ContaId",
                table: "Assinaturas");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId_CategoriaId_DataTransacao",
                table: "Transacoes",
                columns: new[] { "ContaId", "CategoriaId", "DataTransacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId_DataTransacao",
                table: "Transacoes",
                columns: new[] { "ContaId", "DataTransacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId_Tipo_DataTransacao",
                table: "Transacoes",
                columns: new[] { "ContaId", "Tipo", "DataTransacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ContaId_Nome",
                table: "Tags",
                columns: new[] { "ContaId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recorrencias_ContaId_Ativa_ProximaExecucao",
                table: "Recorrencias",
                columns: new[] { "ContaId", "Ativa", "ProximaExecucao" });

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentosMensais_ContaId_Mes_Ano_CategoriaId",
                table: "OrcamentosMensais",
                columns: new[] { "ContaId", "Mes", "Ano", "CategoriaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetasFinanceiras_ContaId_DataInicio",
                table: "MetasFinanceiras",
                columns: new[] { "ContaId", "DataInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_ContaId_Ativa_ProximaCobranca",
                table: "Assinaturas",
                columns: new[] { "ContaId", "Ativa", "ProximaCobranca" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transacoes_ContaId_CategoriaId_DataTransacao",
                table: "Transacoes");

            migrationBuilder.DropIndex(
                name: "IX_Transacoes_ContaId_DataTransacao",
                table: "Transacoes");

            migrationBuilder.DropIndex(
                name: "IX_Transacoes_ContaId_Tipo_DataTransacao",
                table: "Transacoes");

            migrationBuilder.DropIndex(
                name: "IX_Tags_ContaId_Nome",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Recorrencias_ContaId_Ativa_ProximaExecucao",
                table: "Recorrencias");

            migrationBuilder.DropIndex(
                name: "IX_OrcamentosMensais_ContaId_Mes_Ano_CategoriaId",
                table: "OrcamentosMensais");

            migrationBuilder.DropIndex(
                name: "IX_MetasFinanceiras_ContaId_DataInicio",
                table: "MetasFinanceiras");

            migrationBuilder.DropIndex(
                name: "IX_Assinaturas_ContaId_Ativa_ProximaCobranca",
                table: "Assinaturas");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId",
                table: "Transacoes",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ContaId",
                table: "Tags",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recorrencias_ContaId",
                table: "Recorrencias",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentosMensais_ContaId",
                table: "OrcamentosMensais",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_MetasFinanceiras_ContaId",
                table: "MetasFinanceiras",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_ContaId",
                table: "Assinaturas",
                column: "ContaId");
        }
    }
}
