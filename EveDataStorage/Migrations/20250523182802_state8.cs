using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EveDataStorage.Migrations
{
    /// <inheritdoc />
    public partial class state8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MineralMinings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    CorporationId = table.Column<int>(type: "INTEGER", nullable: false),
                    TypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    SolarSystemId = table.Column<long>(type: "INTEGER", nullable: false),
                    SolarSystemId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MineralMinings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MineralMinings_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "character_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MineralMinings_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "corporation_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MineralMinings_SolarSystems_SolarSystemId1",
                        column: x => x.SolarSystemId1,
                        principalTable: "SolarSystems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MineralMinings_CharacterId",
                table: "MineralMinings",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_MineralMinings_CorporationId",
                table: "MineralMinings",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_MineralMinings_SolarSystemId1",
                table: "MineralMinings",
                column: "SolarSystemId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MineralMinings");
        }
    }
}
