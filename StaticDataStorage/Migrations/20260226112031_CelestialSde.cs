using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaticDataStorage.Migrations
{
    /// <inheritdoc />
    public partial class CelestialSde : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Constellations",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FactionID = table.Column<int>(type: "INTEGER", nullable: false),
                    WormholeClassID = table.Column<int>(type: "INTEGER", nullable: false),
                    RegionID = table.Column<int>(type: "INTEGER", nullable: false),
                    SolarSystemIDs = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Constellations", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Moons",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    СelestialIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    OrbitID = table.Column<int>(type: "INTEGER", nullable: false),
                    SolarSystemID = table.Column<int>(type: "INTEGER", nullable: false),
                    TypeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moons", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Planets",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    СelestialIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    Radius = table.Column<double>(type: "REAL", nullable: false),
                    SolarSystemID = table.Column<int>(type: "INTEGER", nullable: false),
                    TypeID = table.Column<int>(type: "INTEGER", nullable: false),
                    MoonIDs = table.Column<string>(type: "TEXT", nullable: false),
                    IdSovResource = table.Column<int>(type: "INTEGER", nullable: false),
                    SovResourceValue = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planets", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    FactionID = table.Column<int>(type: "INTEGER", nullable: false),
                    WormholeClassID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "SolarSystems",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ConstellationID = table.Column<int>(type: "INTEGER", nullable: false),
                    RegionID = table.Column<int>(type: "INTEGER", nullable: false),
                    SecurityStatus = table.Column<double>(type: "REAL", nullable: false),
                    StarID = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanetIDs = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarSystems", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Constellations");

            migrationBuilder.DropTable(
                name: "Moons");

            migrationBuilder.DropTable(
                name: "Planets");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "SolarSystems");
        }
    }
}
