using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Auth_Turkeysoftware.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateSequence(
                name: "admin_action_sequence");

            migrationBuilder.CreateSequence<int>(
                name: "hist_aspnet_users_sequence");

            migrationBuilder.CreateSequence<int>(
                name: "test_data_sequence");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR", maxLength: 128, nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributedCache",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Value = table.Column<byte[]>(type: "bytea", nullable: false),
                    ExpiresAtTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SlidingExpiration = table.Column<long>(type: "bigint", nullable: true),
                    AbsoluteExpiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_hist_aspnet_users",
                schema: "auth",
                columns: table => new
                {
                    IdMudanca = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"hist_aspnet_users_sequence\"')"),
                    Id = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DbOperationType = table.Column<char>(type: "character(1)", nullable: false),
                    dt_inclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_hist_aspnet_users", x => x.IdMudanca);
                },
                comment: "Tracks the changes in the AspNetUsers table");

            migrationBuilder.CreateTable(
                name: "tb_hist_usuar_login",
                schema: "auth",
                columns: table => new
                {
                    id_sessao = table.Column<string>(type: "text", nullable: false),
                    fk_id_usuario = table.Column<string>(type: "VARCHAR", maxLength: 255, nullable: false),
                    dt_inclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nm_pais = table.Column<string>(type: "VARCHAR", maxLength: 60, nullable: true),
                    nm_estado = table.Column<string>(type: "VARCHAR", maxLength: 60, nullable: true),
                    nm_provedora = table.Column<string>(type: "VARCHAR", maxLength: 60, nullable: true),
                    nr_ip = table.Column<string>(type: "VARCHAR", maxLength: 50, nullable: false),
                    nm_platform = table.Column<string>(type: "VARCHAR", maxLength: 30, nullable: true),
                    ds_userAgent = table.Column<string>(type: "VARCHAR", maxLength: 150, nullable: true),
                    DbOperationType = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tb_log_admin_action",
                columns: table => new
                {
                    id_action = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"admin_action_sequence\"')"),
                    fk_id_usuario = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nm_classe_metodo_executado = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ds_argumentos = table.Column<string>(type: "text", nullable: false),
                    dt_inclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_log_admin_action", x => x.id_action);
                });

            migrationBuilder.CreateTable(
                name: "tb_test",
                schema: "auth",
                columns: table => new
                {
                    id_test = table.Column<string>(type: "text", nullable: false, defaultValueSql: "nextval('\"test_data_sequence\"')"),
                    nr_number = table.Column<int>(type: "integer", nullable: false),
                    ds_string = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_test", x => x.id_test);
                });

            migrationBuilder.CreateTable(
                name: "tb_usuar_session",
                schema: "auth",
                columns: table => new
                {
                    id_sessao = table.Column<string>(type: "text", nullable: false),
                    fk_id_usuario = table.Column<string>(type: "VARCHAR", maxLength: 255, nullable: false),
                    ds_refresh_token = table.Column<string>(type: "text", nullable: false),
                    st_token = table.Column<char>(type: "character(1)", nullable: false, comment: "A - Ativo | I - Inativo"),
                    dt_inclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dt_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nm_pais = table.Column<string>(type: "VARCHAR", maxLength: 60, nullable: true),
                    nm_estado = table.Column<string>(type: "VARCHAR", maxLength: 60, nullable: true),
                    nm_provedora = table.Column<string>(type: "VARCHAR", maxLength: 60, nullable: true),
                    nr_ip = table.Column<string>(type: "VARCHAR", maxLength: 50, nullable: false),
                    nm_platform = table.Column<string>(type: "VARCHAR", maxLength: 30, nullable: true),
                    ds_userAgent = table.Column<string>(type: "VARCHAR", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_usuar_session", x => x.id_sessao);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fk_idusuar_session",
                schema: "auth",
                table: "tb_usuar_session",
                column: "fk_id_usuario");

            migrationBuilder.Sql("CREATE FUNCTION \"LC_TRIGGER_AFTER_INSERT_APPLICATIONUSER\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_APPLICATIONUSER$\r\nBEGIN\r\n  INSERT INTO \"auth\".\"tb_hist_aspnet_users\" (\"Id\", \"UserName\", \"NormalizedUserName\", \"Email\", \"NormalizedEmail\", \"PasswordHash\", \"PhoneNumber\", \"Name\", \"DbOperationType\", \"dt_inclusao\") SELECT NEW.\"Id\", \r\n  NEW.\"UserName\", \r\n  NEW.\"NormalizedUserName\", \r\n  NEW.\"Email\", \r\n  NEW.\"NormalizedEmail\", \r\n  NEW.\"PasswordHash\", \r\n  NEW.\"PhoneNumber\", \r\n  NEW.\"Name\", \r\n  'I', \r\n  NOW();\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_APPLICATIONUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_APPLICATIONUSER AFTER INSERT\r\nON \"AspNetUsers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"LC_TRIGGER_AFTER_INSERT_APPLICATIONUSER\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"LC_TRIGGER_AFTER_UPDATE_APPLICATIONUSER\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_APPLICATIONUSER$\r\nBEGIN\r\n  INSERT INTO \"auth\".\"tb_hist_aspnet_users\" (\"Id\", \"UserName\", \"NormalizedUserName\", \"Email\", \"NormalizedEmail\", \"PasswordHash\", \"PhoneNumber\", \"Name\", \"DbOperationType\", \"dt_inclusao\") SELECT NEW.\"Id\", \r\n  NEW.\"UserName\", \r\n  NEW.\"NormalizedUserName\", \r\n  NEW.\"Email\", \r\n  NEW.\"NormalizedEmail\", \r\n  NEW.\"PasswordHash\", \r\n  NEW.\"PhoneNumber\", \r\n  NEW.\"Name\", \r\n  'A', \r\n  NOW();\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_APPLICATIONUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_APPLICATIONUSER AFTER UPDATE\r\nON \"AspNetUsers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"LC_TRIGGER_AFTER_UPDATE_APPLICATIONUSER\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"auth\".\"LC_TRIGGER_AFTER_INSERT_USERSESSIONMODEL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_USERSESSIONMODEL$\r\nBEGIN\r\n  INSERT INTO \"auth\".\"tb_hist_usuar_login\" (\"id_sessao\", \"fk_id_usuario\", \"dt_inclusao\", \"nm_estado\", \"nm_provedora\", \"nr_ip\", \"nm_platform\", \"ds_userAgent\", \"DbOperationType\") SELECT NEW.\"id_sessao\", \r\n  NEW.\"fk_id_usuario\", \r\n  NEW.\"dt_inclusao\", \r\n  NEW.\"nm_estado\", \r\n  NEW.\"nm_provedora\", \r\n  NEW.\"nr_ip\", \r\n  NEW.\"nm_platform\", \r\n  NEW.\"ds_userAgent\", \r\n  'I';\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_USERSESSIONMODEL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_USERSESSIONMODEL AFTER INSERT\r\nON \"auth\".\"tb_usuar_session\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"auth\".\"LC_TRIGGER_AFTER_INSERT_USERSESSIONMODEL\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"LC_TRIGGER_AFTER_INSERT_APPLICATIONUSER\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"LC_TRIGGER_AFTER_UPDATE_APPLICATIONUSER\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"auth\".\"LC_TRIGGER_AFTER_INSERT_USERSESSIONMODEL\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DistributedCache",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "tb_hist_aspnet_users",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "tb_hist_usuar_login",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "tb_log_admin_action");

            migrationBuilder.DropTable(
                name: "tb_test",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "tb_usuar_session",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropSequence(
                name: "admin_action_sequence");

            migrationBuilder.DropSequence(
                name: "hist_aspnet_users_sequence");

            migrationBuilder.DropSequence(
                name: "test_data_sequence");
        }
    }
}
