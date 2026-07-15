using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelIvrWhatsappTagsFieldsToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IvrStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopifyTags",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IvrStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShopifyTags",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WhatsAppStatus",
                table: "Orders");
        }
    }
}
