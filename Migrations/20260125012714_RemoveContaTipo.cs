using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PraOndeFoi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContaTipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Contas_ContaId1",
                table: "Transacoes");

            migrationBuilder.DropIndex(
                name: "IX_Transacoes_ContaId1",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "ContaId1",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Contas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContaId1",
                table: "Transacoes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Contas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId1",
                table: "Transacoes",
                column: "ContaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Contas_ContaId1",
                table: "Transacoes",
                column: "ContaId1",
                principalTable: "Contas",
                principalColumn: "Id");
        }
    }
}
