using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PraOndeFoi.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulerAndValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Contas_ContaDestinoId",
                table: "Transacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Contas_Usuarios_UsuarioId",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FusoHorario",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MoedaPreferida",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SenhaHash",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "ContaDestinoId",
                table: "Transacoes",
                newName: "CategoriaId");

            migrationBuilder.RenameColumn(
                name: "Categoria",
                table: "Transacoes",
                newName: "Descricao");

            migrationBuilder.RenameIndex(
                name: "IX_Transacoes_ContaDestinoId",
                table: "Transacoes",
                newName: "IX_Transacoes_CategoriaId");

            migrationBuilder.Sql("ALTER TABLE \"Usuarios\" ALTER COLUMN \"Id\" DROP IDENTITY;");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql("ALTER TABLE \"Transacoes\" ALTER COLUMN \"Tipo\" TYPE integer USING CASE WHEN \"Tipo\" = 'Entrada' THEN 1 WHEN \"Tipo\" = 'Saida' THEN 2 ELSE 0 END;");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "Contas",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "AnexosTransacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    ConteudoTexto = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    TransacaoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnexosTransacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnexosTransacao_Transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalTable: "Transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Predefinida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    ContaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assinaturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    Moeda = table.Column<string>(type: "text", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Frequencia = table.Column<string>(type: "text", nullable: false),
                    IntervaloQuantidade = table.Column<int>(type: "integer", nullable: false),
                    IntervaloUnidade = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProximaCobranca = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    ContaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinaturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetasFinanceiras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    ValorAlvo = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorAtual = table.Column<decimal>(type: "numeric", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContaId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetasFinanceiras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetasFinanceiras_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MetasFinanceiras_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrcamentosMensais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Limite = table.Column<decimal>(type: "numeric", nullable: false),
                    ContaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrcamentosMensais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrcamentosMensais_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrcamentosMensais_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recorrencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    Moeda = table.Column<string>(type: "text", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Frequencia = table.Column<string>(type: "text", nullable: false),
                    IntervaloQuantidade = table.Column<int>(type: "integer", nullable: false),
                    IntervaloUnidade = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DiaDoMes = table.Column<int>(type: "integer", nullable: true),
                    ProximaExecucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    ContaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recorrencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recorrencias_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recorrencias_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransacaoTags",
                columns: table => new
                {
                    TransacaoId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransacaoTags", x => new { x.TransacaoId, x.TagId });
                    table.ForeignKey(
                        name: "FK_TransacaoTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransacaoTags_Transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalTable: "Transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Nome", "Predefinida" },
                values: new object[,]
                {
                    { 1, "Salário", true },
                    { 2, "Freelance", true },
                    { 3, "Aluguel", true },
                    { 4, "Alimentação", true },
                    { 5, "Transporte", true },
                    { 6, "Saúde", true },
                    { 7, "Educação", true },
                    { 8, "Lazer", true },
                    { 9, "Investimentos", true },
                    { 10, "Assinaturas", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnexosTransacao_TransacaoId",
                table: "AnexosTransacao",
                column: "TransacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_CategoriaId",
                table: "Assinaturas",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_ContaId",
                table: "Assinaturas",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_MetasFinanceiras_CategoriaId",
                table: "MetasFinanceiras",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_MetasFinanceiras_ContaId",
                table: "MetasFinanceiras",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentosMensais_CategoriaId",
                table: "OrcamentosMensais",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentosMensais_ContaId",
                table: "OrcamentosMensais",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recorrencias_CategoriaId",
                table: "Recorrencias",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recorrencias_ContaId",
                table: "Recorrencias",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ContaId",
                table: "Tags",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransacaoTags_TagId",
                table: "TransacaoTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Categorias_CategoriaId",
                table: "Transacoes",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Categorias_CategoriaId",
                table: "Transacoes");

            migrationBuilder.DropTable(
                name: "AnexosTransacao");

            migrationBuilder.DropTable(
                name: "Assinaturas");

            migrationBuilder.DropTable(
                name: "MetasFinanceiras");

            migrationBuilder.DropTable(
                name: "OrcamentosMensais");

            migrationBuilder.DropTable(
                name: "Recorrencias");

            migrationBuilder.DropTable(
                name: "TransacaoTags");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "Transacoes",
                newName: "Categoria");

            migrationBuilder.RenameColumn(
                name: "CategoriaId",
                table: "Transacoes",
                newName: "ContaDestinoId");

            migrationBuilder.RenameIndex(
                name: "IX_Transacoes_CategoriaId",
                table: "Transacoes",
                newName: "IX_Transacoes_ContaDestinoId");

            migrationBuilder.DropForeignKey(
                name: "FK_Contas_Usuarios_UsuarioId",
                table: "Contas");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FusoHorario",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MoedaPreferida",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SenhaHash",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("ALTER TABLE \"Transacoes\" ALTER COLUMN \"Tipo\" TYPE text USING \"Tipo\"::text;");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Contas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Contas_Usuarios_UsuarioId",
                table: "Contas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contas_Usuarios_UsuarioId",
                table: "Contas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Contas_ContaDestinoId",
                table: "Transacoes",
                column: "ContaDestinoId",
                principalTable: "Contas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
