using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace lab6.Migrations
{
    /// <inheritdoc />
    public partial class NormalizedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "access",
                table: "membershipplan");

            migrationBuilder.DropColumn(
                name: "price",
                table: "membership");

            migrationBuilder.DropColumn(
                name: "current_enrollment",
                table: "class");

            migrationBuilder.CreateTable(
                name: "facilityzone",
                columns: table => new
                {
                    zone_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("facilityzone_pkey", x => x.zone_id);
                });

            migrationBuilder.CreateTable(
                name: "planaccess",
                columns: table => new
                {
                    plan_id = table.Column<int>(type: "integer", nullable: false),
                    zone_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("planaccess_pkey", x => new { x.plan_id, x.zone_id });
                    table.ForeignKey(
                        name: "planaccess_plan_id_fkey",
                        column: x => x.plan_id,
                        principalTable: "membershipplan",
                        principalColumn: "plan_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "planaccess_zone_id_fkey",
                        column: x => x.zone_id,
                        principalTable: "facilityzone",
                        principalColumn: "zone_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "facilityzone_name_key",
                table: "facilityzone",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planaccess_zone_id",
                table: "planaccess",
                column: "zone_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "planaccess");

            migrationBuilder.DropTable(
                name: "facilityzone");

            migrationBuilder.AddColumn<string>(
                name: "access",
                table: "membershipplan",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "membership",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "current_enrollment",
                table: "class",
                type: "integer",
                nullable: true,
                defaultValue: 0);
        }
    }
}
