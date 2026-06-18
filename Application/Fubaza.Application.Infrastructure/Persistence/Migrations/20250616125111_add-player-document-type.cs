using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addplayerdocumenttype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerDocument_PlayerId",
                table: "PlayerDocument");

            migrationBuilder.AddColumn<int>(
                name: "DocumentType",
                table: "PlayerDocument",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerDocument_PlayerId",
                table: "PlayerDocument",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerDocument_PlayerId",
                table: "PlayerDocument");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "PlayerDocument");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerDocument_PlayerId",
                table: "PlayerDocument",
                column: "PlayerId",
                unique: true);
        }
    }
}
