using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestRepo.Migrations
{
    /// <inheritdoc />
    public partial class removeigncall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_Vols_BookId",
                table: "Vols",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vols_Books_BookId",
                table: "Vols",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vols_Books_BookId",
                table: "Vols");

            migrationBuilder.DropIndex(
                name: "IX_Vols_BookId",
                table: "Vols");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Books",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
