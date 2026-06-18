using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class jerseynumberiscaption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCaption",
                table: "Player",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JerseyNumber",
                table: "Player",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCaption",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "JerseyNumber",
                table: "Player");
        }
    }
}
