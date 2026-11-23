using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vestigium.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMovementReceiverFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceivedByEmployeeNumber",
                table: "Movements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByName",
                table: "Movements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByUserId",
                table: "Movements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedByEmployeeNumber",
                table: "Movements");

            migrationBuilder.DropColumn(
                name: "ReceivedByName",
                table: "Movements");

            migrationBuilder.DropColumn(
                name: "ReceivedByUserId",
                table: "Movements");
        }
    }
}
