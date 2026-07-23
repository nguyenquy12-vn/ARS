using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFolderJdAndCvMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiPriorityNote",
                table: "CvFolders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiWeightAchievement",
                table: "CvFolders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AiWeightEducation",
                table: "CvFolders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AiWeightExperience",
                table: "CvFolders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AiWeightSkills",
                table: "CvFolders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "JdDescription",
                table: "CvFolders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JdRequirements",
                table: "CvFolders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchConcerns",
                table: "CvBankEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MatchScore",
                table: "CvBankEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MatchScoredAt",
                table: "CvBankEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchStrengths",
                table: "CvBankEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchVerdict",
                table: "CvBankEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchedSkills",
                table: "CvBankEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissingSkills",
                table: "CvBankEntries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiPriorityNote",
                table: "CvFolders");

            migrationBuilder.DropColumn(
                name: "AiWeightAchievement",
                table: "CvFolders");

            migrationBuilder.DropColumn(
                name: "AiWeightEducation",
                table: "CvFolders");

            migrationBuilder.DropColumn(
                name: "AiWeightExperience",
                table: "CvFolders");

            migrationBuilder.DropColumn(
                name: "AiWeightSkills",
                table: "CvFolders");

            migrationBuilder.DropColumn(
                name: "JdDescription",
                table: "CvFolders");

            migrationBuilder.DropColumn(
                name: "JdRequirements",
                table: "CvFolders");

            migrationBuilder.DropColumn(
                name: "MatchConcerns",
                table: "CvBankEntries");

            migrationBuilder.DropColumn(
                name: "MatchScore",
                table: "CvBankEntries");

            migrationBuilder.DropColumn(
                name: "MatchScoredAt",
                table: "CvBankEntries");

            migrationBuilder.DropColumn(
                name: "MatchStrengths",
                table: "CvBankEntries");

            migrationBuilder.DropColumn(
                name: "MatchVerdict",
                table: "CvBankEntries");

            migrationBuilder.DropColumn(
                name: "MatchedSkills",
                table: "CvBankEntries");

            migrationBuilder.DropColumn(
                name: "MissingSkills",
                table: "CvBankEntries");
        }
    }
}
