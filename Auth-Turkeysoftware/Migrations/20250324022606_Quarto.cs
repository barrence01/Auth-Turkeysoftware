using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth_Turkeysoftware.Migrations
{
    /// <inheritdoc />
    public partial class Quarto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<char>(
                name: "st_token",
                table: "TB_USUAR_SESSION",
                type: "character(1)",
                nullable: false,
                comment: "A - Ativo | I - Inativo | B - Bloqueado",
                oldClrType: typeof(char),
                oldType: "character(1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<char>(
                name: "st_token",
                table: "TB_USUAR_SESSION",
                type: "character(1)",
                nullable: false,
                oldClrType: typeof(char),
                oldType: "character(1)",
                oldComment: "A - Ativo | I - Inativo | B - Bloqueado");
        }
    }
}
