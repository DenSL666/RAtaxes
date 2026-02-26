using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaticDataStorage.Migrations
{
    /// <inheritdoc />
    public partial class CelestialSdeUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrbitIndex",
                table: "Moons",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrbitIndex",
                table: "Moons");
        }
    }
}
