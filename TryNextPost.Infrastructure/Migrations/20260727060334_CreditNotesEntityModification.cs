using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreditNotesEntityModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "creditNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "creditNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "creditNotes",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "creditNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "creditNotes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "creditNotes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "creditNotes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "creditNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "creditNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "creditNotes");
        }
    }
}
