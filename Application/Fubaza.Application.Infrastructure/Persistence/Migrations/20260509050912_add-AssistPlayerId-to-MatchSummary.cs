using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addAssistPlayerIdtoMatchSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchSummary_Player_PlayerId",
                table: "MatchSummary");

            migrationBuilder.AddColumn<Guid>(
                name: "AssistPlayerId",
                table: "MatchSummary",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchSummary_AssistPlayerId",
                table: "MatchSummary",
                column: "AssistPlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchSummary_Player_AssistPlayerId",
                table: "MatchSummary",
                column: "AssistPlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchSummary_Player_PlayerId",
                table: "MatchSummary",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchSummary_Player_AssistPlayerId",
                table: "MatchSummary");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchSummary_Player_PlayerId",
                table: "MatchSummary");

            migrationBuilder.DropIndex(
                name: "IX_MatchSummary_AssistPlayerId",
                table: "MatchSummary");

            migrationBuilder.DropColumn(
                name: "AssistPlayerId",
                table: "MatchSummary");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchSummary_Player_PlayerId",
                table: "MatchSummary",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id");
        }
    }
}
