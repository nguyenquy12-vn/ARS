using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJdScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiPriorityNote",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiWeightAchievement",
                table: "JobPostings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AiWeightEducation",
                table: "JobPostings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AiWeightExperience",
                table: "JobPostings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AiWeightSkills",
                table: "JobPostings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AiConcerns",
                table: "Application",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiMatchedSkills",
                table: "Application",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiMissingSkills",
                table: "Application",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiRecommendation",
                table: "Application",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiScoredAt",
                table: "Application",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiStrengths",
                table: "Application",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiVerdict",
                table: "Application",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AiConcerns", "AiMatchedSkills", "AiMissingSkills", "AiRecommendation", "AiScoredAt", "AiStrengths", "AiVerdict" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AiConcerns", "AiMatchedSkills", "AiMissingSkills", "AiRecommendation", "AiScoredAt", "AiStrengths", "AiVerdict" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AiConcerns", "AiMatchedSkills", "AiMissingSkills", "AiRecommendation", "AiScoredAt", "AiStrengths", "AiVerdict" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AiConcerns", "AiMatchedSkills", "AiMissingSkills", "AiRecommendation", "AiScoredAt", "AiStrengths", "AiVerdict" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AiPriorityNote", "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills" },
                values: new object[] { null, 15, 10, 35, 40 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiPriorityNote",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "AiWeightAchievement",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "AiWeightEducation",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "AiWeightExperience",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "AiWeightSkills",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "AiConcerns",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "AiMatchedSkills",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "AiMissingSkills",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "AiRecommendation",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "AiScoredAt",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "AiStrengths",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "AiVerdict",
                table: "Application");
        }
    }
}
