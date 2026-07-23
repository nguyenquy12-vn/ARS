using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCvFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "CvBankEntries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CvFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecruiterId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CvFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CvFolders_Users_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CvBankEntries_FolderId",
                table: "CvBankEntries",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_CvFolders_RecruiterId",
                table: "CvFolders",
                column: "RecruiterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CvBankEntries_CvFolders_FolderId",
                table: "CvBankEntries",
                column: "FolderId",
                principalTable: "CvFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CvBankEntries_CvFolders_FolderId",
                table: "CvBankEntries");

            migrationBuilder.DropTable(
                name: "CvFolders");

            migrationBuilder.DropIndex(
                name: "IX_CvBankEntries_FolderId",
                table: "CvBankEntries");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "CvBankEntries");
        }
    }
}
