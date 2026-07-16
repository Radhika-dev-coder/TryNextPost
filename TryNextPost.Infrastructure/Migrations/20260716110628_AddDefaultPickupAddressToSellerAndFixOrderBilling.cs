using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultPickupAddressToSellerAndFixOrderBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_BillingAddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BillingAddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingAddressId",
                table: "Orders");

            migrationBuilder.AddColumn<long>(
                name: "DefaultPickupAddressId",
                table: "Sellers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingAddressLine1",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingAddressLine2",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCompanyName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCountry",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingFirstName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingLastName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingPincode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingState",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBillingSameAsShipping",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "PickupAddressId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_DefaultPickupAddressId",
                table: "Sellers",
                column: "DefaultPickupAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PickupAddressId",
                table: "Orders",
                column: "PickupAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_PickupAddressId",
                table: "Orders",
                column: "PickupAddressId",
                principalTable: "Addresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sellers_Addresses_DefaultPickupAddressId",
                table: "Sellers",
                column: "DefaultPickupAddressId",
                principalTable: "Addresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_PickupAddressId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_Addresses_DefaultPickupAddressId",
                table: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_Sellers_DefaultPickupAddressId",
                table: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PickupAddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DefaultPickupAddressId",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "BillingAddressLine1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingAddressLine2",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingCity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingCompanyName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingCountry",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingFirstName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingLastName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingPincode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingState",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsBillingSameAsShipping",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickupAddressId",
                table: "Orders");

            migrationBuilder.AddColumn<long>(
                name: "BillingAddressId",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BillingAddressId",
                table: "Orders",
                column: "BillingAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_BillingAddressId",
                table: "Orders",
                column: "BillingAddressId",
                principalTable: "Addresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
