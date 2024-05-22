using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestRepo.Migrations
{
    /// <inheritdoc />
    public partial class testinit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Loans_AccountId",
                table: "Loans",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Accounts_AccountId",
                table: "Loans",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Accounts_AccountId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_AccountId",
                table: "Loans");
        }
    }
}
