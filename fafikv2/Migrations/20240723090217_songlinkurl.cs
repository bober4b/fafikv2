﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class songlinkurl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkUrl",
                table: "Songs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkUrl",
                table: "Songs");
        }
    }
}
