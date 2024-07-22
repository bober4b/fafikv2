using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class Songs3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPlayedSongs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlayedSongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPlayedSongs_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPlayedSongs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPlayedSongs_SongId",
                table: "UserPlayedSongs",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPlayedSongs_UserId",
                table: "UserPlayedSongs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPlayedSongs");
        }
    }
}
