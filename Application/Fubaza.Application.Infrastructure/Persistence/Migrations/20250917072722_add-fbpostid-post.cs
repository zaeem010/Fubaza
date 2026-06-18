using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addfbpostidpost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPosted",
                table: "Post");

            migrationBuilder.AddColumn<string>(
                name: "MetaPostId",
                table: "Post",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetaPostId",
                table: "Post");

            migrationBuilder.AddColumn<bool>(
                name: "IsPosted",
                table: "Post",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
