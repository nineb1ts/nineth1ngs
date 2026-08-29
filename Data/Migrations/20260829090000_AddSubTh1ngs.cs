using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nineth1ngs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubTh1ngs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Th1ngs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Th1ngs_ParentId",
                table: "Th1ngs",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Th1ngs_Th1ngs_ParentId",
                table: "Th1ngs",
                column: "ParentId",
                principalTable: "Th1ngs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Th1ngs_Th1ngs_ParentId",
                table: "Th1ngs");

            migrationBuilder.DropIndex(
                name: "IX_Th1ngs_ParentId",
                table: "Th1ngs");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Th1ngs");
        }
    }
}