using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RelatedId = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills" },
                values: new object[] { 15, 10, 35, 40 });

            migrationBuilder.UpdateData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills" },
                values: new object[] { 15, 10, 35, 40 });

            migrationBuilder.UpdateData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills" },
                values: new object[] { 15, 10, 35, 40 });

            migrationBuilder.UpdateData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills" },
                values: new object[] { 15, 10, 35, 40 });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.UpdateData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills" },
                values: new object[] { 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills" },
                values: new object[] { 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills" },
                values: new object[] { 0, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills" },
                values: new object[] { 0, 0, 0, 0 });
        }
    }
}
