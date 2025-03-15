using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth_Turkeysoftware.Migrations
{
    /// <inheritdoc />
    public partial class Terceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nm_classe_executada",
                table: "TB_LOG_ADMIN_ACTION");

            migrationBuilder.RenameColumn(
                name: "nm_metodo_executado",
                table: "TB_LOG_ADMIN_ACTION",
                newName: "nm_classe_metodo_executado");

            migrationBuilder.AlterColumn<string>(
                name: "nm_classe_metodo_executado",
                table: "TB_LOG_ADMIN_ACTION",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nm_classe_metodo_executado",
                table: "TB_LOG_ADMIN_ACTION",
                newName: "nm_metodo_executado");

            migrationBuilder.AlterColumn<string>(
                name: "nm_metodo_executado",
                table: "TB_LOG_ADMIN_ACTION",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "nm_classe_executada",
                table: "TB_LOG_ADMIN_ACTION",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }
    }
}
