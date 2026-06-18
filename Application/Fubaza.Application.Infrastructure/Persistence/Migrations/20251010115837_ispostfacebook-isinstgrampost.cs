using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ispostfacebookisinstgrampost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "PostTarget");

            migrationBuilder.RenameColumn(
                name: "MetaPostId",
                table: "PostTarget",
                newName: "InstagramPostId");

            migrationBuilder.AddColumn<string>(
                name: "FacebookPostId",
                table: "PostTarget",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookPostId",
                table: "PostTarget");

            migrationBuilder.RenameColumn(
                name: "InstagramPostId",
                table: "PostTarget",
                newName: "MetaPostId");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "PostTarget",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
