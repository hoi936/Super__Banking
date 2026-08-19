using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalLink.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileTopUps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MobileTopUps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ProductType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    ProductCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    CardSerial = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    CardPin = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileTopUps", x => x.Id);
                    table.CheckConstraint("CK_MobileTopUps_Amount_Positive", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_MobileTopUps_BankAccounts_SourceAccountId",
                        column: x => x.SourceAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MobileTopUps_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MobileTopUps_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MobileTopUps_CustomerId_CreatedAtUtc",
                table: "MobileTopUps",
                columns: new[] { "CustomerId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MobileTopUps_IdempotencyKey",
                table: "MobileTopUps",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTopUps_ProviderCode",
                table: "MobileTopUps",
                column: "ProviderCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTopUps_SourceAccountId",
                table: "MobileTopUps",
                column: "SourceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTopUps_Status",
                table: "MobileTopUps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTopUps_TransactionId",
                table: "MobileTopUps",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobileTopUps");
        }
    }
}
