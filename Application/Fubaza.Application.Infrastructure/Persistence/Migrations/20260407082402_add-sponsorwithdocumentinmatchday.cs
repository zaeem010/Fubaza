using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addsponsorwithdocumentinmatchday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sponsor",
                table: "Matchday");

            migrationBuilder.CreateTable(
                name: "SponsorDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sponsor = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    MatchdayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorDocument_Matchday_MatchdayId",
                        column: x => x.MatchdayId,
                        principalTable: "Matchday",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorDocument_MatchdayId",
                table: "SponsorDocument",
                column: "MatchdayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SponsorDocument");

            migrationBuilder.AddColumn<string>(
                name: "Sponsor",
                table: "Matchday",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }
    }
}
