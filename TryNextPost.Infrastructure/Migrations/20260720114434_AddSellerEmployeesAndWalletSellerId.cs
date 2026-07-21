using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerEmployeesAndWalletSellerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets");

            migrationBuilder.AddColumn<long>(
                name: "SellerId",
                table: "Wallets",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE w
                SET w.SellerId = s.SellerId
                FROM Wallets w
                INNER JOIN Sellers s ON s.UserId = w.UserId
                WHERE w.SellerId IS NULL;
            ");

            migrationBuilder.Sql(@"
                DELETE t
                FROM Transactions t
                INNER JOIN Wallets w ON w.WalletId = t.WalletId
                WHERE w.SellerId IS NULL;

                DELETE r
                FROM WalletRecharges r
                INNER JOIN Wallets w ON w.WalletId = r.WalletId
                WHERE w.SellerId IS NULL;

                DELETE FROM Wallets WHERE SellerId IS NULL;
            ");

            migrationBuilder.AlterColumn<long>(
                name: "SellerId",
                table: "Wallets",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SellerEmployees",
                columns: table => new
                {
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerEmployees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_SellerEmployees_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "SellerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePermissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePermissions_SellerEmployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "SellerEmployees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_SellerId",
                table: "Wallets",
                column: "SellerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePermissions_EmployeeId_PermissionCode",
                table: "EmployeePermissions",
                columns: new[] { "EmployeeId", "PermissionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerEmployees_Email",
                table: "SellerEmployees",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_SellerEmployees_SellerId",
                table: "SellerEmployees",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerEmployees_UserId",
                table: "SellerEmployees",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Sellers_SellerId",
                table: "Wallets",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "SellerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Sellers_SellerId",
                table: "Wallets");

            migrationBuilder.DropTable(
                name: "EmployeePermissions");

            migrationBuilder.DropTable(
                name: "SellerEmployees");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_SellerId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Wallets");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId",
                unique: true);
        }
    }
}
