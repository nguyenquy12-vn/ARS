using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecruiterId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanySize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Overview = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Users_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecruiterRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruiterRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruiterRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resumes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RawTextContent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resumes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resumes_Users_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPostings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Benefits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JobType = table.Column<int>(type: "int", nullable: false),
                    WorkMode = table.Column<int>(type: "int", nullable: false),
                    JobCategoryId = table.Column<int>(type: "int", nullable: false),
                    MinSalary = table.Column<int>(type: "int", nullable: true),
                    MaxSalary = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Vacancies = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPostings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPostings_JobCategories_JobCategoryId",
                        column: x => x.JobCategoryId,
                        principalTable: "JobCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Application",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobPostingId = table.Column<int>(type: "int", nullable: false),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    CoverLetter = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AiMatchScore = table.Column<int>(type: "int", nullable: true),
                    AiFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Application", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Application_JobPostings_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Application_Resumes_ResumeId",
                        column: x => x.ResumeId,
                        principalTable: "Resumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Application_Users_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "JobCategories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Lập trình viên, Tester, AI Engineer, DevOps...", "Công nghệ thông tin / Phần mềm" },
                    { 2, "Sales Executive, Account Manager, Chăm sóc khách hàng...", "Kinh doanh / Bán hàng" },
                    { 3, "Digital Marketing, Content Creator, SEO, Event Organizer...", "Marketing / Truyền thông" },
                    { 4, "Kế toán tổng hợp, Kiểm toán viên, Phân tích tài chính...", "Tài chính / Kế toán" },
                    { 5, "Tuyển dụng, C&B, Trợ lý, Quản lý văn phòng...", "Hành chính / Nhân sự" },
                    { 6, "UI/UX Designer, Graphic Designer, Video Editor...", "Thiết kế / Đồ họa" },
                    { 7, "Phiên dịch viên Tiếng Anh, Tiếng Trung, Tiếng Nhật...", "Biên phiên dịch / Ngôn ngữ" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Xem danh sách tin tuyển dụng", "ViewJob" },
                    { 2, "Tạo mới tin tuyển dụng", "CreateJob" },
                    { 3, "Chỉnh sửa tin tuyển dụng", "EditJob" },
                    { 4, "Xóa tin tuyển dụng", "DeleteJob" },
                    { 5, "Nộp hồ sơ ứng tuyển (CV)", "ApplyJob" },
                    { 6, "Xem và đánh giá hồ sơ ứng viên", "ReviewCV" },
                    { 7, "Sử dụng trí tuệ nhân tạo (AI) để chấm điểm CV", "EvaluateAI" },
                    { 8, "Quản lý vai trò và phân quyền hệ thống", "ManageRoles" },
                    { 9, "Quản lý danh sách người dùng", "ManageUsers" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Quản trị viên toàn quyền hệ thống", "Admin" },
                    { 2, "Nhà tuyển dụng (Đăng tin, duyệt CV, dùng AI)", "Recruiter" },
                    { 3, "Ứng viên (Tìm việc, nộp CV)", "Candidate" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "PasswordHash", "PhoneNumber", "RoleId", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 20, 14, 1, 29, 759, DateTimeKind.Utc).AddTicks(9995), "admin@ars.com", "Hệ Thống Admin", "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.", "0123456789", 1, "Active" },
                    { 2, new DateTime(2026, 7, 20, 14, 1, 29, 759, DateTimeKind.Utc).AddTicks(9998), "recruiter1@fpt.com", "Nguyễn Văn Tuyển FPT", "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.", "0987654321", 2, "Active" },
                    { 3, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc), "recruiter2@viettel.com", "Trần Thị Duyệt Viettel", "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.", "0912345678", 2, "Active" },
                    { 4, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(2), "candidate1@gmail.com", "Lê Văn Pro .NET", "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.", "0333444555", 3, "Active" },
                    { 5, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(4), "candidate2@gmail.com", "Nguyễn Thị Fresher", "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.", "0333444666", 3, "Active" },
                    { 6, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(6), "candidate3@gmail.com", "Trần Văn Intern", "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.", "0333444777", 3, "Active" },
                    { 7, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(8), "candidate4@gmail.com", "Hoàng Lệ Trái Ngành", "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.", "0333444888", 3, "Active" }
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "CompanyName", "CompanySize", "LogoPath", "Overview", "RecruiterId", "TaxCode", "Website" },
                values: new object[] { 1, "Khu Công nghệ cao Hòa Lạc, Thạch Thất, Hà Nội", "FPT Software", "10000+ nhân viên", "/uploads/logos/fpt-software.png", "Tập đoàn công nghệ hàng đầu Việt Nam.", 2, "0101248141", "https://fpt-software.com" });

            migrationBuilder.InsertData(
                table: "Resumes",
                columns: new[] { "Id", "CandidateId", "CreatedAt", "FilePath", "IsDefault", "RawTextContent", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 4, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(108), "/uploads/cv1.pdf", true, "Kinh nghiệm 3 năm làm việc với C#, chuyên sâu Web API, Entity Framework Core, SQL Server và Docker.", "CV Lê Văn Pro - .NET Developer", null },
                    { 2, 5, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(110), "/uploads/cv2.pdf", true, "Sinh viên mới tốt nghiệp, biết cơ bản về C# và OOP, chưa có kinh nghiệm thực tế hệ thống lớn.", "CV Nguyễn Thị Fresher", null },
                    { 3, 6, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(112), "/uploads/cv3.pdf", true, "Sinh viên năm 4 tìm kiếm vị trí thực tập, biết viết câu lệnh SQL cơ bản, đang học C#.", "CV Trần Văn Intern Backend", null },
                    { 4, 7, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(114), "/uploads/cv4.pdf", true, "Kinh nghiệm 2 năm chạy quảng cáo Facebook, Google Ads, tư vấn chốt đơn hàng.", "CV Hoàng Lệ - Sales Marketing", null }
                });

            migrationBuilder.InsertData(
                table: "JobPostings",
                columns: new[] { "Id", "Benefits", "CompanyId", "CreatedAt", "Description", "ExpiredAt", "JobCategoryId", "JobType", "Location", "MaxSalary", "MinSalary", "Requirements", "Status", "Title", "Vacancies", "WorkMode" },
                values: new object[] { 1, "Lương thưởng tháng 13, bảo hiểm FPT Care, làm việc hybrid.", 1, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(73), "Phát triển các hệ thống Web API quy mô lớn sử dụng .NET 8 và SQL Server.", new DateTime(2026, 8, 19, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(73), 1, 1, "Cầu Giấy, Hà Nội", 25000000, 15000000, "Có kinh nghiệm lập trình C#, hiểu biết về Entity Framework Core, SQL Server. Biết Docker là một lợi thế.", 2, "Kỹ Sư Lập Trình Backend .NET", 3, 3 });

            migrationBuilder.InsertData(
                table: "Application",
                columns: new[] { "Id", "AiFeedback", "AiMatchScore", "AppliedAt", "CandidateId", "CoverLetter", "JobPostingId", "ResumeId", "Status" },
                values: new object[,]
                {
                    { 1, "Hồ sơ hoàn hảo. Ứng viên có đầy đủ kỹ năng cứng về C#, EF Core, SQL và Docker trùng khớp hoàn toàn với JD.", 95, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(347), 4, "Tôi rất mong muốn được làm việc tại FPT.", 1, 1, 2 },
                    { 2, "Ứng viên có kiến thức nền tảng C# nhưng thiếu kinh nghiệm thực tế với SQL và hệ thống lớn theo yêu cầu.", 65, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(350), 5, "Mong công ty cho cơ hội phỏng vấn.", 1, 2, 2 },
                    { 3, "Hồ sơ còn khá yếu, chưa đáp ứng được các tiêu chí kỹ thuật tối thiểu của vị trí hiện tại.", 40, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(352), 6, "Xin thực tập ạ.", 1, 3, 2 },
                    { 4, "Hồ sơ không phù hợp. Ứng viên làm mảng Marketing, hoàn toàn không có kỹ năng lập trình phần mềm.", 10, new DateTime(2026, 7, 20, 14, 1, 29, 760, DateTimeKind.Utc).AddTicks(354), 7, "Tìm kiếm cơ hội mới.", 1, 4, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Application_CandidateId",
                table: "Application",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Application_JobPostingId",
                table: "Application",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_Application_ResumeId",
                table: "Application",
                column: "ResumeId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_RecruiterId",
                table: "Companies",
                column: "RecruiterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TaxCode",
                table: "Companies",
                column: "TaxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_CompanyId",
                table: "JobPostings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_JobCategoryId",
                table: "JobPostings",
                column: "JobCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterRequests_UserId",
                table: "RecruiterRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_CandidateId",
                table: "Resumes",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Application");

            migrationBuilder.DropTable(
                name: "RecruiterRequests");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "JobPostings");

            migrationBuilder.DropTable(
                name: "Resumes");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "JobCategories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
