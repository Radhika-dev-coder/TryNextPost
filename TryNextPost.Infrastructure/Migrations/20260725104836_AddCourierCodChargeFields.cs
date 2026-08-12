using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourierCodChargeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Flat = 1, default value 30 preserves legacy hardcoded COD fee for existing couriers.
            migrationBuilder.AddColumn<int>(
                name: "CodChargeType",
                table: "Couriers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "CodChargeValue",
                table: "Couriers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 30m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodChargeType",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "CodChargeValue",
                table: "Couriers");
        }
    }
}