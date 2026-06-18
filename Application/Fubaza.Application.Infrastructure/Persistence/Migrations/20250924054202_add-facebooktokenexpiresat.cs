using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addfacebooktokenexpiresat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FacebookTokenExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookTokenExpiresAt",
                table: "Users");
        }
    }
}
