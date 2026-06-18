using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class temepleteuserid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Templete",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templete_UserId",
                table: "Templete",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Templete_Users_UserId",
                table: "Templete",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Templete_Users_UserId",
                table: "Templete");

            migrationBuilder.DropIndex(
                name: "IX_Templete_UserId",
                table: "Templete");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Templete");
        }
    }
}
