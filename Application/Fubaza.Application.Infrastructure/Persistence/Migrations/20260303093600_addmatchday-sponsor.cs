using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addmatchdaysponsor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Player",
                newName: "Nationality");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Club",
                newName: "Nationality");

            migrationBuilder.AddColumn<string>(
                name: "Sponsor",
                table: "Matchday",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sponsor",
                table: "Matchday");

            migrationBuilder.RenameColumn(
                name: "Nationality",
                table: "Player",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "Nationality",
                table: "Club",
                newName: "Country");
        }
    }
}
