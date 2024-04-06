using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConfigId",
                table: "ServerConfigs",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "ServerUserId",
                table: "ServerUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServerUserId",
                table: "ServerUsers");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ServerConfigs",
                newName: "ConfigId");
        }
    }
}
