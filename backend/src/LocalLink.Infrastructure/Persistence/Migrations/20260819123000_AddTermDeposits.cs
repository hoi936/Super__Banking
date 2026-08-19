using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalLink.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTermDeposits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TermDeposits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpeningTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaturityTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepositNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AnnualInterestRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TenorMonths = table.Column<int>(type: "int", nullable: false),
                    ExpectedInterestAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidInterestAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    OpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaturityDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermDeposits", x => x.Id);
                    table.CheckConstraint("CK_TermDeposits_ExpectedInterest_NonNegative", "[ExpectedInterestAmount] >= 0");
                    table.CheckConstraint("CK_TermDeposits_InterestRate_NonNegative", "[AnnualInterestRate] >= 0");
                    table.CheckConstraint("CK_TermDeposits_Principal_Positive", "[PrincipalAmount] > 0");
                    table.CheckConstraint("CK_TermDeposits_Tenor_Allowed", "[TenorMonths] IN (1, 3, 12)");
                    table.ForeignKey(
                        name: "FK_TermDeposits_BankAccounts_SourceAccountId",
                        column: x => x.SourceAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermDeposits_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermDeposits_Transactions_MaturityTransactionId",
                        column: x => x.MaturityTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermDeposits_Transactions_OpeningTransactionId",
                        column: x => x.OpeningTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TermDeposits_CustomerId_Status",
                table: "TermDeposits",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TermDeposits_DepositNumber",
                table: "TermDeposits",
                column: "DepositNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermDeposits_IdempotencyKey",
                table: "TermDeposits",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TermDeposits_MaturityDateUtc",
                table: "TermDeposits",
                column: "MaturityDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TermDeposits_MaturityTransactionId",
                table: "TermDeposits",
                column: "MaturityTransactionId",
                unique: true,
                filter: "[MaturityTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TermDeposits_OpeningTransactionId",
                table: "TermDeposits",
                column: "OpeningTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermDeposits_SourceAccountId",
                table: "TermDeposits",
                column: "SourceAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TermDeposits");
        }
    }
}
