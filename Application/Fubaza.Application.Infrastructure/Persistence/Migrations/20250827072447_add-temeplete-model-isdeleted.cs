using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addtemepletemodelisdeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Templete");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Templete",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Templete");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Templete",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
