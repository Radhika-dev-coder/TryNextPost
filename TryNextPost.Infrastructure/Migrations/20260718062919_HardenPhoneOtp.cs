using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenPhoneOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Otps");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Otps",
                newName: "OtpId");

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "Otps",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Otps",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CodeHash",
                table: "Otps",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Otps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedAttempts",
                table: "Otps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Otps",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Otps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Otps",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeHash",
                table: "Otps");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Otps");

            migrationBuilder.DropColumn(
                name: "FailedAttempts",
                table: "Otps");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Otps");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Otps");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Otps");

            migrationBuilder.RenameColumn(
                name: "OtpId",
                table: "Otps",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "Otps",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Otps",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Otps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
