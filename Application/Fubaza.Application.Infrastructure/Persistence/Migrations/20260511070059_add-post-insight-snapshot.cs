using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addpostinsightsnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostInsightSnapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostTargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    ExternalPostId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Likes = table.Column<int>(type: "int", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false),
                    Shares = table.Column<int>(type: "int", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RawResponse = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostInsightSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostInsightSnapshot_PostTarget_PostTargetId",
                        column: x => x.PostTargetId,
                        principalTable: "PostTarget",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostInsightSnapshot_Platform",
                table: "PostInsightSnapshot",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_PostInsightSnapshot_PostTargetId_FetchedAt",
                table: "PostInsightSnapshot",
                columns: new[] { "PostTargetId", "FetchedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostInsightSnapshot");
        }
    }
}
