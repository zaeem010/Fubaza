using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addisconnectedfacebookuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacebookLongLivedToken",
                table: "Users",
                type: "nvarchar(2000)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConnectedFacebook",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookLongLivedToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsConnectedFacebook",
                table: "Users");
        }
    }
}
