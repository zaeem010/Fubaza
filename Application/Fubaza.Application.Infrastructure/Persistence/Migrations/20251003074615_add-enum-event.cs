using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addenumevent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchSummary_EventType_EventTypeId",
                table: "MatchSummary");

            migrationBuilder.DropTable(
                name: "EventType");

            migrationBuilder.DropIndex(
                name: "IX_MatchSummary_EventTypeId",
                table: "MatchSummary");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "MatchSummary");

            migrationBuilder.AddColumn<int>(
                name: "EventType",
                table: "MatchSummary",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "MatchSummary");

            migrationBuilder.AddColumn<Guid>(
                name: "EventTypeId",
                table: "MatchSummary",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "EventType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(264)", maxLength: 264, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventType", x => x.Id);
                });

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
    }
}
