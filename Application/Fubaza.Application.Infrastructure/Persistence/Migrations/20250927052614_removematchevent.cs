using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class removematchevent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchSummary_EventType_EventTypeId1",
                table: "MatchSummary");

            migrationBuilder.DropIndex(
                name: "IX_MatchSummary_EventTypeId1",
                table: "MatchSummary");

            migrationBuilder.DropColumn(
                name: "EventTypeId1",
                table: "MatchSummary");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventTypeId",
                table: "MatchSummary",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchSummary_EventTypeId",
                table: "MatchSummary",
                column: "EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchSummary_EventType_EventTypeId",
                table: "MatchSummary",
                column: "EventTypeId",
                principalTable: "EventType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchSummary_EventType_EventTypeId",
                table: "MatchSummary");

            migrationBuilder.DropIndex(
                name: "IX_MatchSummary_EventTypeId",
                table: "MatchSummary");

            migrationBuilder.AlterColumn<string>(
                name: "EventTypeId",
                table: "MatchSummary",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "EventTypeId1",
                table: "MatchSummary",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchSummary_EventTypeId1",
                table: "MatchSummary",
                column: "EventTypeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchSummary_EventType_EventTypeId1",
                table: "MatchSummary",
                column: "EventTypeId1",
                principalTable: "EventType",
                principalColumn: "Id");
        }
    }
}
