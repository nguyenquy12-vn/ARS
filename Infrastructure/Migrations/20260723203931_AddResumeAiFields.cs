using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeAiFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AiAiYears",
                table: "Resumes",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiAnalyzedAt",
                table: "Resumes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AiIsFresher",
                table: "Resumes",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiName",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSkills",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSummary",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiTitle",
                table: "Resumes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AiTotalYears",
                table: "Resumes",
                type: "float",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Resumes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AiAiYears", "AiAnalyzedAt", "AiIsFresher", "AiName", "AiSkills", "AiSummary", "AiTitle", "AiTotalYears" },
                values: new object[] { null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Resumes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AiAiYears", "AiAnalyzedAt", "AiIsFresher", "AiName", "AiSkills", "AiSummary", "AiTitle", "AiTotalYears" },
                values: new object[] { null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Resumes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AiAiYears", "AiAnalyzedAt", "AiIsFresher", "AiName", "AiSkills", "AiSummary", "AiTitle", "AiTotalYears" },
                values: new object[] { null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Resumes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AiAiYears", "AiAnalyzedAt", "AiIsFresher", "AiName", "AiSkills", "AiSummary", "AiTitle", "AiTotalYears" },
                values: new object[] { null, null, null, null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiAiYears",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AiAnalyzedAt",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AiIsFresher",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AiName",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AiSkills",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AiSummary",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AiTitle",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AiTotalYears",
                table: "Resumes");
        }
    }
}
