using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class NullableUserServerStatsId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerUsers_ServerUsersStats_UserServerStatsId",
                table: "ServerUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserServerStatsId",
                table: "ServerUsers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerUsers_ServerUsersStats_UserServerStatsId",
                table: "ServerUsers",
                column: "UserServerStatsId",
                principalTable: "ServerUsersStats",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerUsers_ServerUsersStats_UserServerStatsId",
                table: "ServerUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserServerStatsId",
                table: "ServerUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerUsers_ServerUsersStats_UserServerStatsId",
                table: "ServerUsers",
                column: "UserServerStatsId",
                principalTable: "ServerUsersStats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
