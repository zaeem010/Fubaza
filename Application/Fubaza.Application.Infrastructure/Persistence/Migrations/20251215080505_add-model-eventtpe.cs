using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addmodeleventtpe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    SportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(264)", maxLength: 264, nullable: true),
                    NameDe = table.Column<string>(type: "nvarchar(264)", maxLength: 264, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventType_Sport_SportId",
                        column: x => x.SportId,
                        principalTable: "Sport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchSummary_EventTypeId",
                table: "MatchSummary",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventType_SportId",
                table: "EventType",
                column: "SportId");

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
    }
}
