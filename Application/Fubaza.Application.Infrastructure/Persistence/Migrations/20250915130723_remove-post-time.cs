using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class removeposttime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "Post");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Post",
                newName: "Caption");

            migrationBuilder.AddColumn<bool>(
                name: "IsFacebook",
                table: "Post",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInstagram",
                table: "Post",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleDateTime",
                table: "Post",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFacebook",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "IsInstagram",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "ScheduleDateTime",
                table: "Post");

            migrationBuilder.RenameColumn(
                name: "Caption",
                table: "Post",
                newName: "Message");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Post",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Time",
                table: "Post",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
