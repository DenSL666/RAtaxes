using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EveDataStorage.Migrations
{
    /// <inheritdoc />
    public partial class state9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MineralMinings_SolarSystems_SolarSystemId1",
                table: "MineralMinings");

            migrationBuilder.DropIndex(
                name: "IX_MineralMinings_SolarSystemId1",
                table: "MineralMinings");

            migrationBuilder.DropColumn(
                name: "SolarSystemId1",
                table: "MineralMinings");

            migrationBuilder.CreateIndex(
                name: "IX_MineralMinings_SolarSystemId",
                table: "MineralMinings",
                column: "SolarSystemId");

            migrationBuilder.AddForeignKey(
                name: "FK_MineralMinings_SolarSystems_SolarSystemId",
                table: "MineralMinings",
                column: "SolarSystemId",
                principalTable: "SolarSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MineralMinings_SolarSystems_SolarSystemId",
                table: "MineralMinings");

            migrationBuilder.DropIndex(
                name: "IX_MineralMinings_SolarSystemId",
                table: "MineralMinings");

            migrationBuilder.AddColumn<int>(
                name: "SolarSystemId1",
                table: "MineralMinings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MineralMinings_SolarSystemId1",
                table: "MineralMinings",
                column: "SolarSystemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MineralMinings_SolarSystems_SolarSystemId1",
                table: "MineralMinings",
                column: "SolarSystemId1",
                principalTable: "SolarSystems",
                principalColumn: "Id");
        }
    }
}
