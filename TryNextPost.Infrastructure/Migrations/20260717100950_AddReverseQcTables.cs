using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReverseQcTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReverseQcDetails",
                columns: table => new
                {
                    ReverseQcDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsUsedProduct = table.Column<bool>(type: "bit", nullable: true),
                    IsDamagedProduct = table.Column<bool>(type: "bit", nullable: true),
                    IsBrandMatched = table.Column<bool>(type: "bit", nullable: true),
                    IsSizeMatched = table.Column<bool>(type: "bit", nullable: true),
                    IsColorMatched = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReverseQcDetails", x => x.ReverseQcDetailId);
                    table.ForeignKey(
                        name: "FK_ReverseQcDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReverseQcImages",
                columns: table => new
                {
                    ReverseQcImageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReverseQcDetailId = table.Column<long>(type: "bigint", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReverseQcImages", x => x.ReverseQcImageId);
                    table.ForeignKey(
                        name: "FK_ReverseQcImages_ReverseQcDetails_ReverseQcDetailId",
                        column: x => x.ReverseQcDetailId,
                        principalTable: "ReverseQcDetails",
                        principalColumn: "ReverseQcDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReverseQcDetails_OrderId",
                table: "ReverseQcDetails",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReverseQcImages_ReverseQcDetailId_DisplayOrder",
                table: "ReverseQcImages",
                columns: new[] { "ReverseQcDetailId", "DisplayOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReverseQcImages");

            migrationBuilder.DropTable(
                name: "ReverseQcDetails");
        }
    }
}
