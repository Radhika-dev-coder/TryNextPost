using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPanKYCTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AadharLast4Digit",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "AadharReferenceId",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "AadharVerified",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "AadharVerifiedOn",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "FailureCode",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "KYCStatus",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "PanHolderName",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "PanNumber",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "PanVerfied",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "PanVerfiedOn",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "SellerKYC");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "SellerKYC");

            migrationBuilder.RenameColumn(
                name: "VerificationReferenceId",
                table: "SellerKYC",
                newName: "PanKYCStatus");

            migrationBuilder.RenameColumn(
                name: "VerificationProvider",
                table: "SellerKYC",
                newName: "BankKYCStatus");

            migrationBuilder.RenameColumn(
                name: "VerficationBy",
                table: "SellerKYC",
                newName: "AadharKYCStatus");

            migrationBuilder.CreateTable(
                name: "AadhaarKYCs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerKycId = table.Column<int>(type: "int", nullable: true),
                    AadharLast4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerficationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AadhaarKYCs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AadhaarKYCs_SellerKYC_SellerKycId",
                        column: x => x.SellerKycId,
                        principalTable: "SellerKYC",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankKYCs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerKycId = table.Column<int>(type: "int", nullable: true),
                    AccountHolderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountNumberMasked = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IFSC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankKYCs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankKYCs_SellerKYC_SellerKycId",
                        column: x => x.SellerKycId,
                        principalTable: "SellerKYC",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PANKYCs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerKycId = table.Column<int>(type: "int", nullable: true),
                    MaskedAadhar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AadharVerified = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PANKYCs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PANKYCs_SellerKYC_SellerKycId",
                        column: x => x.SellerKycId,
                        principalTable: "SellerKYC",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AadhaarKYCs_SellerKycId",
                table: "AadhaarKYCs",
                column: "SellerKycId");

            migrationBuilder.CreateIndex(
                name: "IX_BankKYCs_SellerKycId",
                table: "BankKYCs",
                column: "SellerKycId");

            migrationBuilder.CreateIndex(
                name: "IX_PANKYCs_SellerKycId",
                table: "PANKYCs",
                column: "SellerKycId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AadhaarKYCs");

            migrationBuilder.DropTable(
                name: "BankKYCs");

            migrationBuilder.DropTable(
                name: "PANKYCs");

            migrationBuilder.RenameColumn(
                name: "PanKYCStatus",
                table: "SellerKYC",
                newName: "VerificationReferenceId");

            migrationBuilder.RenameColumn(
                name: "BankKYCStatus",
                table: "SellerKYC",
                newName: "VerificationProvider");

            migrationBuilder.RenameColumn(
                name: "AadharKYCStatus",
                table: "SellerKYC",
                newName: "VerficationBy");

            migrationBuilder.AddColumn<string>(
                name: "AadharLast4Digit",
                table: "SellerKYC",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AadharReferenceId",
                table: "SellerKYC",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AadharVerified",
                table: "SellerKYC",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AadharVerifiedOn",
                table: "SellerKYC",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailureCode",
                table: "SellerKYC",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "SellerKYC",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KYCStatus",
                table: "SellerKYC",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PanHolderName",
                table: "SellerKYC",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanNumber",
                table: "SellerKYC",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanVerfied",
                table: "SellerKYC",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PanVerfiedOn",
                table: "SellerKYC",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "SellerKYC",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "SellerKYC",
                type: "int",
                nullable: true);
        }
    }
}
