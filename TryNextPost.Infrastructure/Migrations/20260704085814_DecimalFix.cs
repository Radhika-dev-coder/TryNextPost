using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DecimalFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Companies_CompanyId1",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CompanyId1",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "CompanyId1",
                table: "Addresses");

            migrationBuilder.AlterColumn<long>(
                name: "CompanyId",
                table: "Addresses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CompanyId",
                table: "Addresses",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Companies_CompanyId",
                table: "Addresses",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Companies_CompanyId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CompanyId",
                table: "Addresses");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "CompanyId1",
                table: "Addresses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CompanyId1",
                table: "Addresses",
                column: "CompanyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Companies_CompanyId1",
                table: "Addresses",
                column: "CompanyId1",
                principalTable: "Companies",
                principalColumn: "CompanyId");
        }
    }
}
