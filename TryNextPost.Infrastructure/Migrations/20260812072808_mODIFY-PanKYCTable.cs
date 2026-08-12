using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mODIFYPanKYCTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PanNumber",
                table: "PANKYCs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PanNumber",
                table: "PANKYCs");
        }
    }
}
