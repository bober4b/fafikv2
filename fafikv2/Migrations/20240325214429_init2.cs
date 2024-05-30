using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AllKarma",
                table: "Users",
                newName: "GlobalKarma");

            migrationBuilder.AddColumn<int>(
                name: "BotInteractionGlobal",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BotInteractionServer",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MessagesCountGlobal",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MessagesCountServer",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserLevel",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ServerConfigs",
                columns: table => new
                {
                    ConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerConfigs", x => x.ConfigId);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servers_ServerConfigs_ConfigId",
                        column: x => x.ConfigId,
                        principalTable: "ServerConfigs",
                        principalColumn: "ConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerUsers",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerUsers", x => new { x.UserId, x.ServerId });
                    table.ForeignKey(
                        name: "FK_ServerUsers_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServerUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ConfigId",
                table: "Servers",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerUsers_ServerId",
                table: "ServerUsers",
                column: "ServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerUsers");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "ServerConfigs");

            migrationBuilder.DropColumn(
                name: "BotInteractionGlobal",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BotInteractionServer",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MessagesCountGlobal",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MessagesCountServer",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserLevel",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "GlobalKarma",
                table: "Users",
                newName: "AllKarma");
        }
    }
}
