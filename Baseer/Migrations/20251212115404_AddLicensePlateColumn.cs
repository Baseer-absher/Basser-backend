using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseer.Migrations
{
    /// <inheritdoc />
    public partial class AddLicensePlateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "EmergencyReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "EmergencyReports");
        }
    }
}
