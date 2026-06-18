using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class lanagaugecodeusersetendefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "Users",
                type: "nvarchar(128)",
                nullable: true,
                defaultValue: "en",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "Users",
                type: "nvarchar(128)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldNullable: true,
                oldDefaultValue: "en");
        }
    }
}
