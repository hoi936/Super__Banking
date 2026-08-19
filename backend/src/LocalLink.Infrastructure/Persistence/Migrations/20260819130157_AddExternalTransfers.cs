using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalLink.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalBankCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ExternalBankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DestinationAccountNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    DestinationAccountName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DestinationType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalTransfers", x => x.Id);
                    table.CheckConstraint("CK_ExternalTransfers_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_ExternalTransfers_DestinationType_Allowed", "[DestinationType] IN ('ACCOUNT', 'CARD')");
                    table.CheckConstraint("CK_ExternalTransfers_Fee_NonNegative", "[FeeAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_ExternalTransfers_BankAccounts_SourceAccountId",
                        column: x => x.SourceAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExternalTransfers_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalTransfers_ExternalBankCode_CreatedAtUtc",
                table: "ExternalTransfers",
                columns: new[] { "ExternalBankCode", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalTransfers_IdempotencyKey",
                table: "ExternalTransfers",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalTransfers_SourceAccountId_CreatedAtUtc",
                table: "ExternalTransfers",
                columns: new[] { "SourceAccountId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalTransfers_Status",
                table: "ExternalTransfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalTransfers_TransactionId",
                table: "ExternalTransfers",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalTransfers");
        }
    }
}
