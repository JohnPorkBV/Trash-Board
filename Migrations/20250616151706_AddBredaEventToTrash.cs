using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrashBoard.Migrations
{
    /// <inheritdoc />
    public partial class AddBredaEventToTrash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BredaEventName",
                table: "TrashDetections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBredaEvent",
                table: "TrashDetections",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BredaEventName",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "IsBredaEvent",
                table: "TrashDetections");
        }
    }
}
