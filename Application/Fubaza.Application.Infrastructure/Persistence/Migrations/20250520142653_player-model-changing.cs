using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class playermodelchanging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Club_ClubProfileId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Player_PlayerProfileId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ClubProfileId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PlayerProfileId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClubProfileId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlayerProfileId",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Player",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Player_UserId",
                table: "Player",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Player_Users_UserId",
                table: "Player",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Player_Users_UserId",
                table: "Player");

            migrationBuilder.DropIndex(
                name: "IX_Player_UserId",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Player");

            migrationBuilder.AddColumn<Guid>(
                name: "ClubProfileId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlayerProfileId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClubProfileId",
                table: "Users",
                column: "ClubProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PlayerProfileId",
                table: "Users",
                column: "PlayerProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Club_ClubProfileId",
                table: "Users",
                column: "ClubProfileId",
                principalTable: "Club",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Player_PlayerProfileId",
                table: "Users",
                column: "PlayerProfileId",
                principalTable: "Player",
                principalColumn: "Id");
        }
    }
}
