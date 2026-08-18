using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalLink.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImproveAuthenticationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RefreshTokens_TokenHash",
                table: "RefreshTokens");
        }
    }
}
