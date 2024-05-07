using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestRepo.Migrations
{
    /// <inheritdoc />
    public partial class modelbuilderchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vols_Books_BookId",
                table: "Vols");

            migrationBuilder.DropIndex(
                name: "IX_Vols_BookId",
                table: "Vols");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
