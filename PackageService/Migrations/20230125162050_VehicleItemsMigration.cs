using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageService.Migrations
{
    /// <inheritdoc />
    public partial class VehicleItemsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Items");

            migrationBuilder.CreateTable(
                name: "VehicleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleItems_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleItems_ItemId",
                table: "VehicleItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleItems_VehicleId",
                table: "VehicleItems",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleItems");

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
