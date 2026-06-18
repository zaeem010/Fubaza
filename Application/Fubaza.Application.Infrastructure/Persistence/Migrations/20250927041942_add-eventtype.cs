using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addeventtype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "MatchSummary",
                newName: "EventTypeId");

            migrationBuilder.AlterColumn<string>(
                name: "Minute",
                table: "MatchSummary",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MatchSummary",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventTypeId1",
                table: "MatchSummary",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MatchStartDateTime",
                table: "Matchday",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EventType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(264)", maxLength: 264, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventType", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchSummary_EventType_EventTypeId1",
                table: "MatchSummary");

            migrationBuilder.DropTable(
                name: "EventType");

            migrationBuilder.DropIndex(
                name: "IX_MatchSummary_EventTypeId1",
                table: "MatchSummary");

            migrationBuilder.DropColumn(
                name: "EventTypeId1",
                table: "MatchSummary");

            migrationBuilder.RenameColumn(
                name: "EventTypeId",
                table: "MatchSummary",
                newName: "EventType");

            migrationBuilder.AlterColumn<string>(
                name: "Minute",
                table: "MatchSummary",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MatchSummary",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MatchStartDateTime",
                table: "Matchday",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");
        }
    }
}
