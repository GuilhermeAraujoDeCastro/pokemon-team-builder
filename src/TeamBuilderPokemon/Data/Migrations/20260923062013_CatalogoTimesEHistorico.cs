using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeamBuilderPokemon.Data.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoTimesEHistorico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pokemons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type1 = table.Column<string>(type: "TEXT", nullable: false),
                    Type2 = table.Column<string>(type: "TEXT", nullable: true),
                    DexNumber = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pokemons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SnapshotJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamRevisions_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    PokemonId = table.Column<int>(type: "INTEGER", nullable: false),
                    SlotNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamSlots_Pokemons_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamSlots_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Pokemons",
                columns: new[] { "Id", "DexNumber", "Name", "Type1", "Type2" },
                values: new object[,]
                {
                    { 1, 1, "Bulbasaur", "Grass", "Poison" },
                    { 2, 4, "Charmander", "Fire", null },
                    { 3, 7, "Squirtle", "Water", null },
                    { 4, 25, "Pikachu", "Electric", null },
                    { 5, 133, "Eevee", "Normal", null },
                    { 6, 66, "Machop", "Fighting", null },
                    { 7, 92, "Gastly", "Ghost", "Poison" },
                    { 8, 95, "Onix", "Rock", "Ground" },
                    { 9, 130, "Gyarados", "Water", "Flying" },
                    { 10, 65, "Alakazam", "Psychic", null },
                    { 11, 123, "Scyther", "Bug", "Flying" },
                    { 12, 149, "Dragonite", "Dragon", "Flying" },
                    { 13, 197, "Umbreon", "Dark", null },
                    { 14, 208, "Steelix", "Steel", "Ground" },
                    { 15, 175, "Togepi", "Fairy", null },
                    { 16, 143, "Snorlax", "Normal", null },
                    { 17, 144, "Articuno", "Ice", "Flying" },
                    { 18, 37, "Vulpix", "Fire", null },
                    { 19, 60, "Poliwag", "Water", null },
                    { 20, 81, "Magnemite", "Electric", "Steel" },
                    { 21, 43, "Oddish", "Grass", "Poison" },
                    { 22, 104, "Cubone", "Ground", null },
                    { 23, 41, "Zubat", "Poison", "Flying" },
                    { 24, 96, "Drowzee", "Psychic", null },
                    { 25, 58, "Growlithe", "Fire", null },
                    { 26, 74, "Geodude", "Rock", "Ground" },
                    { 27, 98, "Krabby", "Water", null },
                    { 28, 63, "Abra", "Psychic", null },
                    { 29, 27, "Sandshrew", "Ground", null },
                    { 30, 39, "Jigglypuff", "Normal", "Fairy" },
                    { 31, 179, "Mareep", "Electric", null },
                    { 32, 246, "Larvitar", "Rock", "Ground" },
                    { 33, 359, "Absol", "Dark", null },
                    { 34, 447, "Riolu", "Fighting", null },
                    { 35, 361, "Snorunt", "Ice", null },
                    { 36, 443, "Gible", "Dragon", "Ground" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemons_Name",
                table: "Pokemons",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamRevisions_TeamId",
                table: "TeamRevisions",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSlots_PokemonId",
                table: "TeamSlots",
                column: "PokemonId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSlots_TeamId_SlotNumber",
                table: "TeamSlots",
                columns: new[] { "TeamId", "SlotNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamRevisions");

            migrationBuilder.DropTable(
                name: "TeamSlots");

            migrationBuilder.DropTable(
                name: "Pokemons");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
