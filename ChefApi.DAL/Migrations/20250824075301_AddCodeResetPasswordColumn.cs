using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChefApi.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeResetPasswordColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CodeReset",
                table: "User",
                newName: "CodeResetPassword");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CodeResetPassword",
                table: "User",
                newName: "CodeReset");
        }
    }
}
