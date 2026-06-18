using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addplayerclub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PlayingPosition_PlayingPositionId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Sport_SportId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ClubId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ClubId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClubId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlayingSince",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StrongFoot",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "SportId",
                table: "Users",
                newName: "PlayerProfileId");

            migrationBuilder.RenameColumn(
                name: "PlayingPositionId",
                table: "Users",
                newName: "ClubProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_SportId",
                table: "Users",
                newName: "IX_Users_PlayerProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_PlayingPositionId",
                table: "Users",
                newName: "IX_Users_ClubProfileId");

            migrationBuilder.CreateTable(
                name: "Club",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Club", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeightKg = table.Column<int>(type: "int", nullable: true),
                    HeightCm = table.Column<int>(type: "int", nullable: true),
                    StrongFoot = table.Column<int>(type: "int", nullable: true),
                    CurrentClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlayingPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Player_Club_CurrentClubId",
                        column: x => x.CurrentClubId,
                        principalTable: "Club",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Player_PlayingPosition_PlayingPositionId",
                        column: x => x.PlayingPositionId,
                        principalTable: "PlayingPosition",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Player_Sport_SportId",
                        column: x => x.SportId,
                        principalTable: "Sport",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlayerClubHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerClubHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerClubHistory_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayerClubHistory_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Player_CurrentClubId",
                table: "Player",
                column: "CurrentClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_PlayingPositionId",
                table: "Player",
                column: "PlayingPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_SportId",
                table: "Player",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerClubHistory_ClubId",
                table: "PlayerClubHistory",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerClubHistory_PlayerId",
                table: "PlayerClubHistory",
                column: "PlayerId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Club_ClubProfileId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Player_PlayerProfileId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "PlayerClubHistory");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "Club");

            migrationBuilder.RenameColumn(
                name: "PlayerProfileId",
                table: "Users",
                newName: "SportId");

            migrationBuilder.RenameColumn(
                name: "ClubProfileId",
                table: "Users",
                newName: "PlayingPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_PlayerProfileId",
                table: "Users",
                newName: "IX_Users_SportId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_ClubProfileId",
                table: "Users",
                newName: "IX_Users_PlayingPositionId");

            migrationBuilder.AddColumn<Guid>(
                name: "ClubId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(325)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeightCm",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayingSince",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StrongFoot",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeightKg",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClubId",
                table: "Users",
                column: "ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PlayingPosition_PlayingPositionId",
                table: "Users",
                column: "PlayingPositionId",
                principalTable: "PlayingPosition",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Sport_SportId",
                table: "Users",
                column: "SportId",
                principalTable: "Sport",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ClubId",
                table: "Users",
                column: "ClubId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
