using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mODIFYBakKYCTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "BankKYCs",
                newName: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "BankKYCs",
                newName: "TransactionId");
        }
    }
}
