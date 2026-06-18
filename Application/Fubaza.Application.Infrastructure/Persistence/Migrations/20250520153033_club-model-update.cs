using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class clubmodelupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Club",
                newName: "FullName");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Club",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Club_UserId",
                table: "Club",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Club_Users_UserId",
                table: "Club",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Club_Users_UserId",
                table: "Club");

            migrationBuilder.DropIndex(
                name: "IX_Club_UserId",
                table: "Club");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Club");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Club",
                newName: "Name");
        }
    }
}
