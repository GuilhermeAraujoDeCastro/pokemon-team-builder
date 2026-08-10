using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamBuilderPokemon.Data.Migrations
{
    /// <inheritdoc />
    public partial class OwnerQuarantineColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Teams",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "LegacyIsPublic",
                table: "Teams",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacyOwnerId",
                table: "Teams",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Teams SET UserId = COALESCE(UserId, LegacyOwnerId, ''),
                    IsPublic = COALESCE(LegacyIsPublic, IsPublic);
                """);
            migrationBuilder.DropColumn(
                name: "LegacyIsPublic",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "LegacyOwnerId",
                table: "Teams");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Teams",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
