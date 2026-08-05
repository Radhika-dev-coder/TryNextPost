using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourierRateCards_CourierId_FromZoneId_ToZoneId_WeightFromGrams_WeightToGrams_ServiceCode",
                table: "CourierRateCards");

            migrationBuilder.AddColumn<long>(
                name: "CourierId",
                table: "PincodeZoneMappings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                table: "CourierServiceabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CodCharge",
                table: "CourierRateCards",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CodPercentage",
                table: "CourierRateCards",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelSurchargePercent",
                table: "CourierRateCards",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HandlingCharge",
                table: "CourierRateCards",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCodAvailable",
                table: "CourierRateCards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxDays",
                table: "CourierRateCards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinDays",
                table: "CourierRateCards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumCharge",
                table: "CourierRateCards",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "CourierRateCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "CourierRateCards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RtoCharge",
                table: "CourierRateCards",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ServiceType",
                table: "CourierRateCards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PincodeZoneMappings_CourierId",
                table: "PincodeZoneMappings",
                column: "CourierId");

            migrationBuilder.CreateIndex(
                name: "IX_CourierServiceabilities_CourierId_ZoneId",
                table: "CourierServiceabilities",
                columns: new[] { "CourierId", "ZoneId" });

            migrationBuilder.CreateIndex(
                name: "IX_CourierServiceabilities_ZoneId",
                table: "CourierServiceabilities",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_CourierRateCards_CourierId_ServiceType_FromZoneId_ToZoneId_WeightFromGrams_WeightToGrams_ServiceCode",
                table: "CourierRateCards",
                columns: new[] { "CourierId", "ServiceType", "FromZoneId", "ToZoneId", "WeightFromGrams", "WeightToGrams", "ServiceCode" });

            migrationBuilder.AddForeignKey(
                name: "FK_CourierServiceabilities_Zones_ZoneId",
                table: "CourierServiceabilities",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "ZoneId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PincodeZoneMappings_Couriers_CourierId",
                table: "PincodeZoneMappings",
                column: "CourierId",
                principalTable: "Couriers",
                principalColumn: "CourierId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourierServiceabilities_Zones_ZoneId",
                table: "CourierServiceabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_PincodeZoneMappings_Couriers_CourierId",
                table: "PincodeZoneMappings");

            migrationBuilder.DropIndex(
                name: "IX_PincodeZoneMappings_CourierId",
                table: "PincodeZoneMappings");

            migrationBuilder.DropIndex(
                name: "IX_CourierServiceabilities_CourierId_ZoneId",
                table: "CourierServiceabilities");

            migrationBuilder.DropIndex(
                name: "IX_CourierServiceabilities_ZoneId",
                table: "CourierServiceabilities");

            migrationBuilder.DropIndex(
                name: "IX_CourierRateCards_CourierId_ServiceType_FromZoneId_ToZoneId_WeightFromGrams_WeightToGrams_ServiceCode",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "CourierId",
                table: "PincodeZoneMappings");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "CourierServiceabilities");

            migrationBuilder.DropColumn(
                name: "CodCharge",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "CodPercentage",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "FuelSurchargePercent",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "HandlingCharge",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "IsCodAvailable",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "MaxDays",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "MinDays",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "MinimumCharge",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "RtoCharge",
                table: "CourierRateCards");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "CourierRateCards");

            migrationBuilder.CreateIndex(
                name: "IX_CourierRateCards_CourierId_FromZoneId_ToZoneId_WeightFromGrams_WeightToGrams_ServiceCode",
                table: "CourierRateCards",
                columns: new[] { "CourierId", "FromZoneId", "ToZoneId", "WeightFromGrams", "WeightToGrams", "ServiceCode" });
        }
    }
}
