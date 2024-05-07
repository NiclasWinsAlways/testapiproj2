using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestRepo.Migrations
{
    /// <inheritdoc />
    public partial class addbookprobfix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BooksProgress_Accounts_AccountId",
                table: "BooksProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_BooksProgress_Books_BookId",
                table: "BooksProgress");

            migrationBuilder.AddForeignKey(
                name: "FK_BooksProgress_Accounts_AccountId",
                table: "BooksProgress",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BooksProgress_Books_BookId",
                table: "BooksProgress",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BooksProgress_Accounts_AccountId",
                table: "BooksProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_BooksProgress_Books_BookId",
                table: "BooksProgress");

            migrationBuilder.AddForeignKey(
                name: "FK_BooksProgress_Accounts_AccountId",
                table: "BooksProgress",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BooksProgress_Books_BookId",
                table: "BooksProgress",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
