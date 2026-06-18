using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addtemepletemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Templete",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(264)", maxLength: 264, nullable: false),
                    JsonTemeplete = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templete", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Templete_Sport_SportId",
                        column: x => x.SportId,
                        principalTable: "Sport",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TempleteDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TempleteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempleteDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TempleteDocument_Templete_TempleteId",
                        column: x => x.TempleteId,
                        principalTable: "Templete",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Templete_SportId",
                table: "Templete",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_TempleteDocument_TempleteId",
                table: "TempleteDocument",
                column: "TempleteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TempleteDocument");

            migrationBuilder.DropTable(
                name: "Templete");
        }
    }
}
