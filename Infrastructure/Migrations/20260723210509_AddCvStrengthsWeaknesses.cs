using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCvStrengthsWeaknesses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiStrengths",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiWeaknesses",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Strengths",
                table: "CvBankEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Weaknesses",
                table: "CvBankEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Resumes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AiStrengths", "AiWeaknesses" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Resumes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AiStrengths", "AiWeaknesses" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Resumes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AiStrengths", "AiWeaknesses" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Resumes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AiStrengths", "AiWeaknesses" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiStrengths",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AiWeaknesses",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "Strengths",
                table: "CvBankEntries");

            migrationBuilder.DropColumn(
                name: "Weaknesses",
                table: "CvBankEntries");
        }
    }
}
