using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vestigium.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMaterialInventoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Materials_Barcode",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_PartNumber",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "RequiresInspection",
                table: "Materials");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Materials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Rack",
                table: "Materials",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_PartNumber",
                table: "Materials",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Rack",
                table: "Materials",
                column: "Rack");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Materials_PartNumber",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_Rack",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Rack",
                table: "Materials");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Materials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresInspection",
                table: "Materials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Barcode",
                table: "Materials",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_PartNumber",
                table: "Materials",
                column: "PartNumber",
                unique: true);
        }
    }
}
