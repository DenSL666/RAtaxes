using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EveDataStorage.Migrations
{
    /// <inheritdoc />
    public partial class State3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CharacterMains_Characters_character_id",
                table: "CharacterMains");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CharacterMains",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "CharacterMains");

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterMains_Characters_character_id",
                table: "CharacterMains",
                column: "character_id",
                principalTable: "Characters",
                principalColumn: "character_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
