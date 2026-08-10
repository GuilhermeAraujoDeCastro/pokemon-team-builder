using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamBuilderPokemon.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceTeamOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A migration anterior tornou UserId anulável. Preserve o vínculo e
            // a visibilidade legados antes de impor a FK, sem apagar órfãos.
            migrationBuilder.Sql("""
                UPDATE Teams
                SET LegacyOwnerId = UserId, LegacyIsPublic = IsPublic,
                    UserId = NULL, IsPublic = 0
                WHERE UserId IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM AspNetUsers u WHERE u.Id = Teams.UserId);
                """);
            migrationBuilder.CreateIndex(
                name: "IX_Teams_UserId",
                table: "Teams",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_UserId",
                table: "Teams",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_UserId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_UserId",
                table: "Teams");
        }
    }
}
