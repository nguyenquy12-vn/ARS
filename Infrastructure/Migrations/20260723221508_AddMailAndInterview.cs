using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMailAndInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SmtpEnableSsl",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SmtpFromEmail",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsername",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InterviewAt",
                table: "Application",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterviewNote",
                table: "Application",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "InterviewAt", "InterviewNote" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "InterviewAt", "InterviewNote" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "InterviewAt", "InterviewNote" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "InterviewAt", "InterviewNote" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SmtpEnableSsl", "SmtpFromEmail", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpUsername" },
                values: new object[] { true, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "SmtpEnableSsl", "SmtpFromEmail", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpUsername" },
                values: new object[] { true, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "SmtpEnableSsl", "SmtpFromEmail", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpUsername" },
                values: new object[] { true, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "SmtpEnableSsl", "SmtpFromEmail", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpUsername" },
                values: new object[] { true, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "SmtpEnableSsl", "SmtpFromEmail", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpUsername" },
                values: new object[] { true, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "SmtpEnableSsl", "SmtpFromEmail", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpUsername" },
                values: new object[] { true, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "SmtpEnableSsl", "SmtpFromEmail", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpUsername" },
                values: new object[] { true, null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpEnableSsl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SmtpFromEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SmtpUsername",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InterviewAt",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "InterviewNote",
                table: "Application");
        }
    }
}
