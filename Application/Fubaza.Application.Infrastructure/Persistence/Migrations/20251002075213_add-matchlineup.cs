using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addmatchlineup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "MatchStartDateTime",
                table: "Matchday",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateTable(
                name: "MatchLineUp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchdayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchLineUp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchLineUp_Matchday_MatchdayId",
                        column: x => x.MatchdayId,
                        principalTable: "Matchday",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchLineUp_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineUp_MatchdayId",
                table: "MatchLineUp",
                column: "MatchdayId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineUp_PlayerId",
                table: "MatchLineUp",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchLineUp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "MatchStartDateTime",
                table: "Matchday",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
