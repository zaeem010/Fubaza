using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addplayerdocumet1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDocument_Player_PlayerId",
                table: "UserDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDocument",
                table: "UserDocument");

            migrationBuilder.RenameTable(
                name: "UserDocument",
                newName: "PlayerDocument");

            migrationBuilder.RenameIndex(
                name: "IX_UserDocument_PlayerId",
                table: "PlayerDocument",
                newName: "IX_PlayerDocument_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayerDocument",
                table: "PlayerDocument",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerDocument_Player_PlayerId",
                table: "PlayerDocument",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerDocument_Player_PlayerId",
                table: "PlayerDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayerDocument",
                table: "PlayerDocument");

            migrationBuilder.RenameTable(
                name: "PlayerDocument",
                newName: "UserDocument");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerDocument_PlayerId",
                table: "UserDocument",
                newName: "IX_UserDocument_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDocument",
                table: "UserDocument",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDocument_Player_PlayerId",
                table: "UserDocument",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
