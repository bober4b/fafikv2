using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class ServerStatasUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServerUsers_UserServerStatsId",
                table: "ServerUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "ServerUserId",
                table: "ServerUsersStats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ServerUsers_UserServerStatsId",
                table: "ServerUsers",
                column: "UserServerStatsId",
                unique: true,
                filter: "[UserServerStatsId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServerUsers_UserServerStatsId",
                table: "ServerUsers");

            migrationBuilder.DropColumn(
                name: "ServerUserId",
                table: "ServerUsersStats");

            migrationBuilder.CreateIndex(
                name: "IX_ServerUsers_UserServerStatsId",
                table: "ServerUsers",
                column: "UserServerStatsId");
        }
    }
}
