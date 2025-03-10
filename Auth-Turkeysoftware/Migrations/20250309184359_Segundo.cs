using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth_Turkeysoftware.Migrations
{
    /// <inheritdoc />
    public partial class Segundo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_COD_USUAR_SESSION",
                table: "TB_USUAR_SESSION",
                column: "fk_id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_COD_USUAR_SESSION",
                table: "TB_USUAR_SESSION");
        }
    }
}
