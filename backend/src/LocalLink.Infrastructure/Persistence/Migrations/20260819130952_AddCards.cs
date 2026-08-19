using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalLink.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkedAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardNumberMasked = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    LastFourDigits = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: false),
                    CardholderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CardType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    DailyLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    OnlinePaymentEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ContactlessEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ExpiryMonth = table.Column<int>(type: "int", nullable: false),
                    ExpiryYear = table.Column<int>(type: "int", nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.CheckConstraint("CK_Cards_DailyLimit_Positive", "[DailyLimit] > 0");
                    table.CheckConstraint("CK_Cards_ExpiryMonth_Valid", "[ExpiryMonth] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_Cards_ExpiryYear_Valid", "[ExpiryYear] BETWEEN 2026 AND 2099");
                    table.CheckConstraint("CK_Cards_MonthlyLimit_Gte_DailyLimit", "[MonthlyLimit] >= [DailyLimit]");
                    table.CheckConstraint("CK_Cards_MonthlyLimit_Positive", "[MonthlyLimit] > 0");
                    table.ForeignKey(
                        name: "FK_Cards_BankAccounts_LinkedAccountId",
                        column: x => x.LinkedAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardNumberMasked",
                table: "Cards",
                column: "CardNumberMasked",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CustomerId_Status",
                table: "Cards",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_LinkedAccountId",
                table: "Cards",
                column: "LinkedAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");
        }
    }
}
