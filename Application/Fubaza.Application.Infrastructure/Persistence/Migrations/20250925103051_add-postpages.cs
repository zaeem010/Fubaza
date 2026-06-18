using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addpostpages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "MetaPostId",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "PageId",
                table: "Post");

            migrationBuilder.CreateTable(
                name: "PostPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PageName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstagramId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacebookPostId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstagramPostId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostPages_Post_PostId",
                        column: x => x.PostId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostPages_PostId",
                table: "PostPages",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostPages");

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Post",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaPostId",
                table: "Post",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PageId",
                table: "Post",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
