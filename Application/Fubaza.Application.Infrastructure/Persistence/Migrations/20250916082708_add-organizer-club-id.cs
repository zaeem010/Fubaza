using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addorganizerclubid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matchday_Club_ClubId",
                table: "Matchday");

            migrationBuilder.RenameColumn(
                name: "ClubId",
                table: "Matchday",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Matchday_ClubId",
                table: "Matchday",
                newName: "IX_Matchday_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizerClubId",
                table: "Matchday",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matchday_OrganizerClubId",
                table: "Matchday",
                column: "OrganizerClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matchday_Club_OrganizerClubId",
                table: "Matchday",
                column: "OrganizerClubId",
                principalTable: "Club",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matchday_Users_UserId",
                table: "Matchday",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matchday_Club_OrganizerClubId",
                table: "Matchday");

            migrationBuilder.DropForeignKey(
                name: "FK_Matchday_Users_UserId",
                table: "Matchday");

            migrationBuilder.DropIndex(
                name: "IX_Matchday_OrganizerClubId",
                table: "Matchday");

            migrationBuilder.DropColumn(
                name: "OrganizerClubId",
                table: "Matchday");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Matchday",
                newName: "ClubId");

            migrationBuilder.RenameIndex(
                name: "IX_Matchday_UserId",
                table: "Matchday",
                newName: "IX_Matchday_ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matchday_Club_ClubId",
                table: "Matchday",
                column: "ClubId",
                principalTable: "Club",
                principalColumn: "Id");
        }
    }
}
