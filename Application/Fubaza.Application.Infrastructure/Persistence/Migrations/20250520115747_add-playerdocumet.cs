using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fubaza.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addplayerdocumet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDocument_Users_UserId",
                table: "UserDocument");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserDocument",
                newName: "PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_UserDocument_UserId",
                table: "UserDocument",
                newName: "IX_UserDocument_PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDocument_Player_PlayerId",
                table: "UserDocument",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDocument_Player_PlayerId",
                table: "UserDocument");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "UserDocument",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserDocument_PlayerId",
                table: "UserDocument",
                newName: "IX_UserDocument_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDocument_Users_UserId",
                table: "UserDocument",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
