using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addstartyearendyear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "PlayerClubHistory");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PlayerClubHistory");

            migrationBuilder.AddColumn<int>(
                name: "EndYear",
                table: "PlayerClubHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartYear",
                table: "PlayerClubHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Player",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Player",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndYear",
                table: "PlayerClubHistory");

            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "PlayerClubHistory");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Player");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "PlayerClubHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "PlayerClubHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
