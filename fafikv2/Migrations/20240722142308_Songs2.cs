using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class Songs2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "Songs",
                newName: "Genres");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Genres",
                table: "Songs",
                newName: "Tags");
        }
    }
}
