using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addInstagramloginfieldsinuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstagramLongLivedToken",
                table: "Users",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InstagramTokenExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConnectedInstagram",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstagramLongLivedToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstagramTokenExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsConnectedInstagram",
                table: "Users");
        }
    }
}
