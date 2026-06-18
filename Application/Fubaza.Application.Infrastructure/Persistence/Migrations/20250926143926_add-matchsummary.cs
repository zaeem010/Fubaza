using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addmatchsummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MatchStartDateTime",
                table: "Matchday",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MatchSummary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Minute = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MatchdayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchSummary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchSummary_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatchSummary_Matchday_MatchdayId",
                        column: x => x.MatchdayId,
                        principalTable: "Matchday",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchSummary_ClubId",
                table: "MatchSummary",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchSummary_MatchdayId",
                table: "MatchSummary",
                column: "MatchdayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchSummary");

            migrationBuilder.DropColumn(
                name: "MatchStartDateTime",
                table: "Matchday");
        }
    }
}
