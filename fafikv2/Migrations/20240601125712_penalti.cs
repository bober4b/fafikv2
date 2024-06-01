using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fafikv2.Migrations
{
    /// <inheritdoc />
    public partial class penalti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPenaltyDate",
                table: "ServerUsersStats",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Penalties",
                table: "ServerUsersStats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPenaltyDate",
                table: "ServerUsersStats");

            migrationBuilder.DropColumn(
                name: "Penalties",
                table: "ServerUsersStats");
        }
    }
}
