using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Application",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 1,
                column: "CancelReason",
                value: null);

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 2,
                column: "CancelReason",
                value: null);

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 3,
                column: "CancelReason",
                value: null);

            migrationBuilder.UpdateData(
                table: "Application",
                keyColumn: "Id",
                keyValue: 4,
                column: "CancelReason",
                value: null);

            migrationBuilder.InsertData(
                table: "JobPostings",
                columns: new[] { "Id", "Benefits", "CompanyId", "CreatedAt", "Description", "ExpiredAt", "JobCategoryId", "JobType", "Location", "MaxSalary", "MinSalary", "Requirements", "Status", "Title", "Vacancies", "WorkMode" },
                values: new object[,]
                {
                    { 2, "Thưởng theo KPIs, môi trường trẻ trung năng động.", 1, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Lên kế hoạch và triển khai các chiến dịch quảng cáo trên nền tảng Digital (Facebook, Google).", new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, "Thanh Xuân, Hà Nội", 20000000, 10000000, "Tối thiểu 1 năm kinh nghiệm chạy Ads. Có khả năng sáng tạo nội dung.", 2, "Chuyên viên Marketing Digital", 2, 1 },
                    { 3, "Hoa hồng cao, phụ cấp ăn trưa và đi lại.", 1, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Tư vấn sản phẩm dịch vụ của công ty đến với khách hàng qua điện thoại.", new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, "Đống Đa, Hà Nội", 15000000, 7000000, "Giọng nói chuẩn, giao tiếp tốt, không yêu cầu kinh nghiệm (được đào tạo).", 2, "Nhân viên Telesales", 5, 1 },
                    { 4, "Hỗ trợ dấu mộc thực tập, trợ cấp 3 triệu/tháng, cơ hội lên nhân viên chính thức.", 1, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Tham gia phát triển các tính năng Front-end cho dự án công ty bằng ReactJS.", new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3, "Quận 1, TP. HCM", 5000000, 3000000, "Sinh viên năm cuối hoặc mới ra trường, nắm chắc HTML/CSS/JS cơ bản.", 2, "Thực tập sinh ReactJS", 4, 2 },
                    { 5, "Chế độ BHYT, BHXH đầy đủ, thưởng lễ tết hấp dẫn.", 1, new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Chịu trách nhiệm kiểm tra đối chiếu số liệu, lập báo cáo tài chính hàng tháng/quý.", new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, "Hải Châu, Đà Nẵng", 18000000, 12000000, "Tốt nghiệp đại học chuyên ngành Kế toán, trên 3 năm kinh nghiệm làm tổng hợp.", 2, "Kế toán tổng hợp", 1, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Application");
        }
    }
}
