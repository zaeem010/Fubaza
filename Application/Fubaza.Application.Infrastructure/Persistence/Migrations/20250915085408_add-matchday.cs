using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addmatchday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matchday",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchdayNumber = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OpponentClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Referee = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AssistantReferee1 = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AssistantReferee2 = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchday", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matchday_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Matchday_Club_OpponentClubId",
                        column: x => x.OpponentClubId,
                        principalTable: "Club",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matchday_ClubId",
                table: "Matchday",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Matchday_OpponentClubId",
                table: "Matchday",
                column: "OpponentClubId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matchday");
        }
    }
}
