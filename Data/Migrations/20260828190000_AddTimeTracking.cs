using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nineth1ngs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ElapsedSeconds",
                table: "Th1ngs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimerStartedAt",
                table: "Th1ngs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElapsedSeconds",
                table: "Th1ngs");

            migrationBuilder.DropColumn(
                name: "TimerStartedAt",
                table: "Th1ngs");
        }
    }
}