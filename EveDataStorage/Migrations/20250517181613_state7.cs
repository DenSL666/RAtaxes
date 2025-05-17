using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EveDataStorage.Migrations
{
    /// <inheritdoc />
    public partial class state7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Constellations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    RegionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Constellations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Constellations_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolarSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ConstellationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolarSystems_Constellations_ConstellationId",
                        column: x => x.ConstellationId,
                        principalTable: "Constellations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Constellations_RegionId",
                table: "Constellations",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolarSystems_ConstellationId",
                table: "SolarSystems",
                column: "ConstellationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolarSystems");

            migrationBuilder.DropTable(
                name: "Constellations");

            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
