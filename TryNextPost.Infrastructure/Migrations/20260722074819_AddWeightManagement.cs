using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductWeightFreezes",
                columns: table => new
                {
                    ProductWeightFreezeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LengthCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BreadthCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HeightCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WeightGrams = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AutoApply = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ActionRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActionedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActionedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWeightFreezes", x => x.ProductWeightFreezeId);
                    table.ForeignKey(
                        name: "FK_ProductWeightFreezes_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "SellerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeightDiscrepancies",
                columns: table => new
                {
                    WeightDiscrepancyId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerId = table.Column<long>(type: "bigint", nullable: false),
                    ShipmentId = table.Column<long>(type: "bigint", nullable: true),
                    OrderId = table.Column<long>(type: "bigint", nullable: true),
                    AwbNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CourierId = table.Column<long>(type: "bigint", nullable: true),
                    CourierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EnteredWeightGrams = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedWeightGrams = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WeightCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WeightAppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DisputeRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisputedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightDiscrepancies", x => x.WeightDiscrepancyId);
                    table.ForeignKey(
                        name: "FK_WeightDiscrepancies_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeightDiscrepancies_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "SellerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeightDiscrepancies_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductWeightFreezes_ProductId",
                table: "ProductWeightFreezes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWeightFreezes_SellerId",
                table: "ProductWeightFreezes",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWeightFreezes_SellerId_ProductId",
                table: "ProductWeightFreezes",
                columns: new[] { "SellerId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductWeightFreezes_Status",
                table: "ProductWeightFreezes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WeightDiscrepancies_AwbNumber",
                table: "WeightDiscrepancies",
                column: "AwbNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WeightDiscrepancies_OrderId",
                table: "WeightDiscrepancies",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightDiscrepancies_SellerId",
                table: "WeightDiscrepancies",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightDiscrepancies_ShipmentId",
                table: "WeightDiscrepancies",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightDiscrepancies_Status",
                table: "WeightDiscrepancies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WeightDiscrepancies_WeightAppliedDate",
                table: "WeightDiscrepancies",
                column: "WeightAppliedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductWeightFreezes");

            migrationBuilder.DropTable(
                name: "WeightDiscrepancies");
        }
    }
}
