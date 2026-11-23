using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vestigium.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegisteredByName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegisteredByName",
                table: "Movements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegisteredByName",
                table: "Movements");
        }
    }
}
