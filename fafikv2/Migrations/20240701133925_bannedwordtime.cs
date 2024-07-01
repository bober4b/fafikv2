using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class bannedwordtime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Time",
                table: "BannedWords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Time",
                table: "BannedWords");
        }
    }
}
