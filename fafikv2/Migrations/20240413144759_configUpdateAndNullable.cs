using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class configUpdateAndNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_ServerConfigs_ConfigId",
                table: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_Servers_ConfigId",
                table: "Servers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "ServerUsersStats",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "ServerId",
                table: "ServerConfigs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerConfigs_ServerId",
                table: "ServerConfigs",
                column: "ServerId",
                unique: true,
                filter: "[ServerId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerConfigs_Servers_ServerId",
                table: "ServerConfigs",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerConfigs_Servers_ServerId",
                table: "ServerConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ServerConfigs_ServerId",
                table: "ServerConfigs");

            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "ServerConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "ServerUsersStats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ConfigId",
                table: "Servers",
                column: "ConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_ServerConfigs_ConfigId",
                table: "Servers",
                column: "ConfigId",
                principalTable: "ServerConfigs",
                principalColumn: "Id");
        }
    }
}
