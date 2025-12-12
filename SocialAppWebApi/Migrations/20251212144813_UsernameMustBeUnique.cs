using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialAppWebApi.Migrations
{
    /// <inheritdoc />
    public partial class UsernameMustBeUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Created",
                table: "PostLikes",
                newName: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "PostLikes",
                newName: "Created");
        }
    }
}
