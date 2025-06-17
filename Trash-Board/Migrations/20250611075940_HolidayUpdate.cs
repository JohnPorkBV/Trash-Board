using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrashBoard.Migrations
{
    /// <inheritdoc />
    public partial class HolidayUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HolidayName",
                table: "TrashDetections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHoliday",
                table: "TrashDetections",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HolidayName",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "IsHoliday",
                table: "TrashDetections");
        }
    }
}
