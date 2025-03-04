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
            migrationBuilder.RenameColumn(
                name: "nm_device",
                table: "TB_USUAR_SESSION",
                newName: "nm_platform");

            migrationBuilder.AddColumn<string>(
                name: "ds_userAgent",
                table: "TB_USUAR_SESSION",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ds_userAgent",
                table: "TB_USUAR_SESSION");

            migrationBuilder.RenameColumn(
                name: "nm_platform",
                table: "TB_USUAR_SESSION",
                newName: "nm_device");
        }
    }
}
