using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class FK_Document_Uni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UniversityId",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UniversityId",
                table: "Documents",
                column: "UniversityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Universities_UniversityId",
                table: "Documents",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Universities_UniversityId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_UniversityId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "UniversityId",
                table: "Documents");
        }
    }
}
