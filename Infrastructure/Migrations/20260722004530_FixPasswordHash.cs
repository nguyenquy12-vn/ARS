using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                column: "PasswordHash",
                value: "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                column: "PasswordHash",
                value: "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                column: "PasswordHash",
                value: "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                column: "PasswordHash",
                value: "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.");
        }
    }
}
