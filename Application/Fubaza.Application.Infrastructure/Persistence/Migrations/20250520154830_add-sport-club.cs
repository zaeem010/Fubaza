using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addsportclub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SportId",
                table: "Club",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Club_SportId",
                table: "Club",
                column: "SportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Club_Sport_SportId",
                table: "Club",
                column: "SportId",
                principalTable: "Sport",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Club_Sport_SportId",
                table: "Club");

            migrationBuilder.DropIndex(
                name: "IX_Club_SportId",
                table: "Club");

            migrationBuilder.DropColumn(
                name: "SportId",
                table: "Club");
        }
    }
}
