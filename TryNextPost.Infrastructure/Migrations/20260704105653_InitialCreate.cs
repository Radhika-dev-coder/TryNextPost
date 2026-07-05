using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Sellers_SellerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SellerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SellerId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SellerId",
                table: "AspNetUsers",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Sellers_SellerId",
                table: "AspNetUsers",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "SellerId");
        }
    }
}
