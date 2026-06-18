using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updatetemepletemodels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TempleteDocument_TempleteId",
                table: "TempleteDocument");

            migrationBuilder.DropColumn(
                name: "JsonTemeplete",
                table: "Templete");

            migrationBuilder.AddColumn<int>(
                name: "DocumentType",
                table: "TempleteDocument",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TempleteDocument_TempleteId",
                table: "TempleteDocument",
                column: "TempleteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TempleteDocument_TempleteId",
                table: "TempleteDocument");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "TempleteDocument");

            migrationBuilder.AddColumn<string>(
                name: "JsonTemeplete",
                table: "Templete",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TempleteDocument_TempleteId",
                table: "TempleteDocument",
                column: "TempleteId",
                unique: true);
        }
    }
}
