using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalLink.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoanApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisbursementAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisbursementTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AnnualInterestRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    ReviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisbursedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplications", x => x.Id);
                    table.CheckConstraint("CK_LoanApplications_ApprovedAmount_Positive", "[ApprovedAmount] IS NULL OR [ApprovedAmount] > 0");
                    table.CheckConstraint("CK_LoanApplications_InterestRate_NonNegative", "[AnnualInterestRate] >= 0");
                    table.CheckConstraint("CK_LoanApplications_MonthlyIncome_Positive", "[MonthlyIncome] > 0");
                    table.CheckConstraint("CK_LoanApplications_RequestedAmount_Positive", "[RequestedAmount] > 0");
                    table.CheckConstraint("CK_LoanApplications_Term_Valid", "[TermMonths] BETWEEN 6 AND 60");
                    table.ForeignKey(
                        name: "FK_LoanApplications_BankAccounts_DisbursementAccountId",
                        column: x => x.DisbursementAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanApplications_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanApplications_Transactions_DisbursementTransactionId",
                        column: x => x.DisbursementTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanApplications_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_ApplicationNumber",
                table: "LoanApplications",
                column: "ApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_CustomerId_Status",
                table: "LoanApplications",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_DisbursementAccountId",
                table: "LoanApplications",
                column: "DisbursementAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_DisbursementTransactionId",
                table: "LoanApplications",
                column: "DisbursementTransactionId",
                unique: true,
                filter: "[DisbursementTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_ReviewedByUserId",
                table: "LoanApplications",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_Status",
                table: "LoanApplications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanApplications");
        }
    }
}
