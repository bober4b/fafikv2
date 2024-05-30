using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserServerStatsId",
                table: "ServerUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ServerUsers_UserServerStatsId",
                table: "ServerUsers",
                column: "UserServerStatsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerUsers_ServerUsersStats_UserServerStatsId",
                table: "ServerUsers",
                column: "UserServerStatsId",
                principalTable: "ServerUsersStats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerUsers_ServerUsersStats_UserServerStatsId",
                table: "ServerUsers");

            migrationBuilder.DropIndex(
                name: "IX_ServerUsers_UserServerStatsId",
                table: "ServerUsers");

            migrationBuilder.DropColumn(
                name: "UserServerStatsId",
                table: "ServerUsers");
        }
    }
}
