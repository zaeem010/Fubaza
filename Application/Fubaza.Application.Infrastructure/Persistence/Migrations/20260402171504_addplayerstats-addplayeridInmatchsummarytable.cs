using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addplayerstatsaddplayeridInmatchsummarytable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlayerId",
                table: "MatchSummary",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlayerStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchSummaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerStats_EventType_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerStats_MatchSummary_MatchSummaryId",
                        column: x => x.MatchSummaryId,
                        principalTable: "MatchSummary",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayerStats_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchSummary_PlayerId",
                table: "MatchSummary",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_EventTypeId",
                table: "PlayerStats",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_MatchSummaryId",
                table: "PlayerStats",
                column: "MatchSummaryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerId",
                table: "PlayerStats",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchSummary_Player_PlayerId",
                table: "MatchSummary",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchSummary_Player_PlayerId",
                table: "MatchSummary");

            migrationBuilder.DropTable(
                name: "PlayerStats");

            migrationBuilder.DropIndex(
                name: "IX_MatchSummary_PlayerId",
                table: "MatchSummary");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "MatchSummary");
        }
    }
}
