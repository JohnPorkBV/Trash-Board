using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrashBoard.Migrations
{
    /// <inheritdoc />
    public partial class AddTrashDetections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemperatureCelsius",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "WeatherCondition",
                table: "TrashDetections");

            migrationBuilder.RenameColumn(
                name: "TrashType",
                table: "TrashDetections",
                newName: "DetectedObject");

            migrationBuilder.AlterColumn<float>(
                name: "Humidity",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ConfidenceScore",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "TrashDetections",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Hour",
                table: "TrashDetections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Precipitation",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Temp",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Windforce",
                table: "TrashDetections",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Hour",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Precipitation",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Temp",
                table: "TrashDetections");

            migrationBuilder.DropColumn(
                name: "Windforce",
                table: "TrashDetections");

            migrationBuilder.RenameColumn(
                name: "DetectedObject",
                table: "TrashDetections",
                newName: "TrashType");

            migrationBuilder.AlterColumn<double>(
                name: "Humidity",
                table: "TrashDetections",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<double>(
                name: "TemperatureCelsius",
                table: "TrashDetections",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeatherCondition",
                table: "TrashDetections",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
