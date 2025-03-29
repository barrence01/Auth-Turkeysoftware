using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth_Turkeysoftware.Migrations
{
    /// <inheritdoc />
    public partial class Quinto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "test_data_sequence");

            migrationBuilder.CreateTable(
                name: "TB_TEST",
                columns: table => new
                {
                    id_test = table.Column<string>(type: "text", nullable: false, defaultValueSql: "nextval('\"test_data_sequence\"')"),
                    nr_number = table.Column<int>(type: "integer", nullable: false),
                    ds_string = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_TEST", x => x.id_test);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_TEST");

            migrationBuilder.DropSequence(
                name: "test_data_sequence");
        }
    }
}
