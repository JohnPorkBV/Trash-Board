using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrashBoard.Migrations
{
    /// <inheritdoc />
    public partial class confidenceasString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ConfidenceScore",
                table: "TrashDetections",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "ConfidenceScore",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
