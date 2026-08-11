using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResetAllPasswordsTo123456 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set every user's password to "123456" (BCrypt, workFactor 12)
            migrationBuilder.Sql(
                "UPDATE Users SET PasswordHash = '$2a$12$z0DMGE1xnyZS5g60TmaduOGISv004dYqW.yLQyfRtbdxdTelyXY6W';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
