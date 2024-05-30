using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class tes5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerUsers_ServerUsersStats_UserServerStatsId",
                table: "ServerUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServerUsers",
                table: "ServerUsers");

            migrationBuilder.DropIndex(
                name: "IX_ServerUsers_UserServerStatsId",
                table: "ServerUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServerUsers",
                table: "ServerUsers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ServerUsersStats_ServerUserId",
                table: "ServerUsersStats",
                column: "ServerUserId",
                unique: true,
                filter: "[ServerUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServerUsers_UserId",
                table: "ServerUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerUsersStats_ServerUsers_ServerUserId",
                table: "ServerUsersStats",
                column: "ServerUserId",
                principalTable: "ServerUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerUsersStats_ServerUsers_ServerUserId",
                table: "ServerUsersStats");

            migrationBuilder.DropIndex(
                name: "IX_ServerUsersStats_ServerUserId",
                table: "ServerUsersStats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServerUsers",
                table: "ServerUsers");

            migrationBuilder.DropIndex(
                name: "IX_ServerUsers_UserId",
                table: "ServerUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServerUsers",
                table: "ServerUsers",
                columns: new[] { "UserId", "ServerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServerUsers_UserServerStatsId",
                table: "ServerUsers",
                column: "UserServerStatsId",
                unique: true,
                filter: "[UserServerStatsId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerUsers_ServerUsersStats_UserServerStatsId",
                table: "ServerUsers",
                column: "UserServerStatsId",
                principalTable: "ServerUsersStats",
                principalColumn: "Id");
        }
    }
}
