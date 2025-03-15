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
            migrationBuilder.AddColumn<string>(
                name: "nm_classe_executada",
                table: "TB_LOG_ADMIN_ACTION",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nm_classe_executada",
                table: "TB_LOG_ADMIN_ACTION");
        }
    }
}
