using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageService.Migrations
{
    /// <inheritdoc />
    public partial class TelegramMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "Vehicles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Vehicles");
        }
    }
}
