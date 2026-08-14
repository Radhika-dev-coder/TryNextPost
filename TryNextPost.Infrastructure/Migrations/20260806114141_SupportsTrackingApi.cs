using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupportsTrackingApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SupportsCancelApi",
                table: "Couriers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsManifestApi",
                table: "Couriers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsTrackingApi",
                table: "Couriers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupportsCancelApi",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "SupportsManifestApi",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "SupportsTrackingApi",
                table: "Couriers");
        }
    }
}
