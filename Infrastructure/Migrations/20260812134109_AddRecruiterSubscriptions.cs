using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruiterSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecruiterSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecruiterId = table.Column<int>(type: "int", nullable: false),
                    PlanCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    AdminNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruiterSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruiterSubscriptions_Users_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterSubscriptions_RecruiterId",
                table: "RecruiterSubscriptions",
                column: "RecruiterId",
                unique: true);

            // Giữ quyền lợi của recruiter đã thanh toán trước khi bảng gói được bổ sung.
            migrationBuilder.Sql(@"
                INSERT INTO RecruiterSubscriptions
                    (RecruiterId, PlanCode, StartedAt, ExpiresAt, UpdatedAt, AdminNote)
                SELECT u.Id, latest.PlanCode, latest.ActivatedAt,
                       DATEADD(day, 30, latest.ActivatedAt), GETUTCDATE(),
                       N'Khởi tạo từ giao dịch thành công gần nhất.'
                FROM Users u
                CROSS APPLY (
                    SELECT TOP (1) p.PlanCode,
                           COALESCE(p.ReviewedAt, p.CreatedAt) AS ActivatedAt
                    FROM PaymentOrders p
                    WHERE p.RecruiterId = u.Id AND p.Status = 'Successful'
                    ORDER BY COALESCE(p.ReviewedAt, p.CreatedAt) DESC, p.Id DESC
                ) latest;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecruiterSubscriptions");
        }
    }
}
