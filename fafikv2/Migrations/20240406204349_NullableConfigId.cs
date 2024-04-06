﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class NullableConfigId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_ServerConfigs_ConfigId",
                table: "Servers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConfigId",
                table: "Servers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_ServerConfigs_ConfigId",
                table: "Servers",
                column: "ConfigId",
                principalTable: "ServerConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_ServerConfigs_ConfigId",
                table: "Servers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConfigId",
                table: "Servers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_ServerConfigs_ConfigId",
                table: "Servers",
                column: "ConfigId",
                principalTable: "ServerConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
