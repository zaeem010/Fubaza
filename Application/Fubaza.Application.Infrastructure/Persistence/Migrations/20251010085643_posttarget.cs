using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class posttarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "IsFacebook",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "IsInstagram",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "MetaPostId",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "PageId",
                table: "Post");

            migrationBuilder.CreateTable(
                name: "PostTarget",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    InstagramBusinessId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsFacebook = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsInstagram = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MetaPostId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTarget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostTarget_Post_PostId",
                        column: x => x.PostId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostTarget_PostId",
                table: "PostTarget",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostTarget");

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Post",
                type: "nvarchar(max)",
                nullable: true);

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
