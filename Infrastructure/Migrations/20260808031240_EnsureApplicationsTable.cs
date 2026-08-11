using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureApplicationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorId = table.Column<int>(type: "int", nullable: true),
                    ActorEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TargetId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

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
                    DisplayedName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SmtpHost = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SmtpPort = table.Column<int>(type: "int", nullable: true),
                    SmtpUsername = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SmtpPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SmtpFromEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SmtpEnableSsl = table.Column<bool>(type: "bit", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    ExternalProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
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
                name: "CvFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecruiterId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JdDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JdRequirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiWeightExperience = table.Column<int>(type: "int", nullable: false),
                    AiWeightSkills = table.Column<int>(type: "int", nullable: false),
                    AiWeightEducation = table.Column<int>(type: "int", nullable: false),
                    AiWeightAchievement = table.Column<int>(type: "int", nullable: false),
                    AiPriorityNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVerifications_Users_UserId",
                        column: x => x.UserId,
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
                    RawTextContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiTotalYears = table.Column<double>(type: "float", nullable: true),
                    AiAiYears = table.Column<double>(type: "float", nullable: true),
                    AiIsFresher = table.Column<bool>(type: "bit", nullable: true),
                    AiSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiStrengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiWeaknesses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiAnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AiWeightExperience = table.Column<int>(type: "int", nullable: false),
                    AiWeightSkills = table.Column<int>(type: "int", nullable: false),
                    AiWeightEducation = table.Column<int>(type: "int", nullable: false),
                    AiWeightAchievement = table.Column<int>(type: "int", nullable: false),
                    AiPriorityNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "CvBankEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecruiterId = table.Column<int>(type: "int", nullable: false),
                    FolderId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurrentTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TotalYearsExperience = table.Column<double>(type: "float", nullable: false),
                    AiYearsExperience = table.Column<double>(type: "float", nullable: false),
                    IsFresher = table.Column<bool>(type: "bit", nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Strengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weaknesses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchScore = table.Column<int>(type: "int", nullable: true),
                    MatchVerdict = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchedSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MissingSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchStrengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchConcerns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchScoredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CvBankEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CvBankEntries_CvFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "CvFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CvBankEntries_Users_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
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
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiMatchScore = table.Column<int>(type: "int", nullable: true),
                    AiFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiVerdict = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiMatchedSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiMissingSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiStrengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiConcerns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiRecommendation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiScoredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InterviewAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InterviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_JobPostings_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_Resumes_ResumeId",
                        column: x => x.ResumeId,
                        principalTable: "Resumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Applications_Users_CandidateId",
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
                columns: new[] { "Id", "Description", "DisplayedName", "Name" },
                values: new object[,]
                {
                    { 1, "Quản trị viên toàn quyền hệ thống", "Quản trị viên", "Admin" },
                    { 2, "Nhà tuyển dụng (Đăng tin, duyệt CV, dùng AI)", "Nhà tuyển dụng", "Recruiter" },
                    { 3, "Ứng viên (Tìm việc, nộp CV)", "Ứng viên", "Candidate" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 8, 1 },
                    { 9, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 3, 2 },
                    { 4, 2 },
                    { 6, 2 },
                    { 7, 2 },
                    { 1, 3 },
                    { 5, 3 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "ExternalId", "ExternalProvider", "FullName", "IsEmailVerified", "PasswordHash", "PhoneNumber", "RoleId", "SmtpEnableSsl", "SmtpFromEmail", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpUsername", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@ars.com", null, null, "Hệ Thống Admin", false, "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy", "0123456789", 1, true, null, null, null, null, null, "Active" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "recruiter1@fpt.com", null, null, "Nguyễn Văn Tuyển FPT", false, "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy", "0987654321", 2, true, null, null, null, null, null, "Active" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "recruiter2@viettel.com", null, null, "Trần Thị Duyệt Viettel", false, "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy", "0912345678", 2, true, null, null, null, null, null, "Active" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "candidate1@gmail.com", null, null, "Lê Văn Pro .NET", false, "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy", "0333444555", 3, true, null, null, null, null, null, "Active" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "candidate2@gmail.com", null, null, "Nguyễn Thị Fresher", false, "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy", "0333444666", 3, true, null, null, null, null, null, "Active" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "candidate3@gmail.com", null, null, "Trần Văn Intern", false, "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy", "0333444777", 3, true, null, null, null, null, null, "Active" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "candidate4@gmail.com", null, null, "Hoàng Lệ Trái Ngành", false, "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy", "0333444888", 3, true, null, null, null, null, null, "Active" }
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "CompanyName", "CompanySize", "LogoPath", "Overview", "RecruiterId", "TaxCode", "Website" },
                values: new object[] { 1, "Khu Công nghệ cao Hòa Lạc, Thạch Thất, Hà Nội", "FPT Software", "10000+ nhân viên", "/uploads/logos/fpt-software.png", "Tập đoàn công nghệ hàng đầu Việt Nam.", 2, "0101248141", "https://fpt-software.com" });

            migrationBuilder.InsertData(
                table: "Resumes",
                columns: new[] { "Id", "AiAiYears", "AiAnalyzedAt", "AiIsFresher", "AiName", "AiSkills", "AiStrengths", "AiSummary", "AiTitle", "AiTotalYears", "AiWeaknesses", "CandidateId", "CreatedAt", "FilePath", "IsDefault", "RawTextContent", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, null, null, null, null, null, null, null, null, null, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/uploads/cv1.pdf", true, "Kinh nghiệm 3 năm làm việc với C#, chuyên sâu Web API, Entity Framework Core, SQL Server và Docker.", "CV Lê Văn Pro - .NET Developer", null },
                    { 2, null, null, null, null, null, null, null, null, null, null, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/uploads/cv2.pdf", true, "Sinh viên mới tốt nghiệp, biết cơ bản về C# và OOP, chưa có kinh nghiệm thực tế hệ thống lớn.", "CV Nguyễn Thị Fresher", null },
                    { 3, null, null, null, null, null, null, null, null, null, null, 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/uploads/cv3.pdf", true, "Sinh viên năm 4 tìm kiếm vị trí thực tập, biết viết câu lệnh SQL cơ bản, đang học C#.", "CV Trần Văn Intern Backend", null },
                    { 4, null, null, null, null, null, null, null, null, null, null, 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/uploads/cv4.pdf", true, "Kinh nghiệm 2 năm chạy quảng cáo Facebook, Google Ads, tư vấn chốt đơn hàng.", "CV Hoàng Lệ - Sales Marketing", null }
                });

            migrationBuilder.InsertData(
                table: "JobPostings",
                columns: new[] { "Id", "AiPriorityNote", "AiWeightAchievement", "AiWeightEducation", "AiWeightExperience", "AiWeightSkills", "Benefits", "CompanyId", "CreatedAt", "Description", "ExpiredAt", "JobCategoryId", "JobType", "Location", "MaxSalary", "MinSalary", "Requirements", "Status", "Title", "Vacancies", "WorkMode" },
                values: new object[,]
                {
                    { 1, null, 15, 10, 35, 40, "Lương thưởng tháng 13, bảo hiểm FPT Care, làm việc hybrid.", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phát triển các hệ thống Web API quy mô lớn sử dụng .NET 8 và SQL Server.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, "Cầu Giấy, Hà Nội", 25000000, 15000000, "Có kinh nghiệm lập trình C#, hiểu biết về Entity Framework Core, SQL Server. Biết Docker là một lợi thế.", 2, "Kỹ Sư Lập Trình Backend .NET", 3, 3 },
                    { 2, null, 15, 10, 35, 40, "Thưởng theo KPIs, môi trường trẻ trung năng động.", 1, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Lên kế hoạch và triển khai các chiến dịch quảng cáo trên nền tảng Digital (Facebook, Google).", new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, "Thanh Xuân, Hà Nội", 20000000, 10000000, "Tối thiểu 1 năm kinh nghiệm chạy Ads. Có khả năng sáng tạo nội dung.", 2, "Chuyên viên Marketing Digital", 2, 1 },
                    { 3, null, 15, 10, 35, 40, "Hoa hồng cao, phụ cấp ăn trưa và đi lại.", 1, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Tư vấn sản phẩm dịch vụ của công ty đến với khách hàng qua điện thoại.", new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, "Đống Đa, Hà Nội", 15000000, 7000000, "Giọng nói chuẩn, giao tiếp tốt, không yêu cầu kinh nghiệm (được đào tạo).", 2, "Nhân viên Telesales", 5, 1 },
                    { 4, null, 15, 10, 35, 40, "Hỗ trợ dấu mộc thực tập, trợ cấp 3 triệu/tháng, cơ hội lên nhân viên chính thức.", 1, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Tham gia phát triển các tính năng Front-end cho dự án công ty bằng ReactJS.", new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3, "Quận 1, TP. HCM", 5000000, 3000000, "Sinh viên năm cuối hoặc mới ra trường, nắm chắc HTML/CSS/JS cơ bản.", 2, "Thực tập sinh ReactJS", 4, 2 },
                    { 5, null, 15, 10, 35, 40, "Chế độ BHYT, BHXH đầy đủ, thưởng lễ tết hấp dẫn.", 1, new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Chịu trách nhiệm kiểm tra đối chiếu số liệu, lập báo cáo tài chính hàng tháng/quý.", new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, "Hải Châu, Đà Nẵng", 18000000, 12000000, "Tốt nghiệp đại học chuyên ngành Kế toán, trên 3 năm kinh nghiệm làm tổng hợp.", 2, "Kế toán tổng hợp", 1, 1 }
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "AiConcerns", "AiFeedback", "AiMatchScore", "AiMatchedSkills", "AiMissingSkills", "AiRecommendation", "AiScoredAt", "AiStrengths", "AiVerdict", "AppliedAt", "CancelReason", "CandidateId", "CoverLetter", "InterviewAt", "InterviewNote", "JobPostingId", "ResumeId", "Status" },
                values: new object[,]
                {
                    { 1, null, "Hồ sơ hoàn hảo. Ứng viên có đầy đủ kỹ năng cứng về C#, EF Core, SQL và Docker trùng khớp hoàn toàn với JD.", 95, null, null, null, null, null, null, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, "Tôi rất mong muốn được làm việc tại FPT.", null, null, 1, 1, 2 },
                    { 2, null, "Ứng viên có kiến thức nền tảng C# nhưng thiếu kinh nghiệm thực tế với SQL và hệ thống lớn theo yêu cầu.", 65, null, null, null, null, null, null, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, "Mong công ty cho cơ hội phỏng vấn.", null, null, 1, 2, 2 },
                    { 3, null, "Hồ sơ còn khá yếu, chưa đáp ứng được các tiêu chí kỹ thuật tối thiểu của vị trí hiện tại.", 40, null, null, null, null, null, null, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, "Xin thực tập ạ.", null, null, 1, 3, 2 },
                    { 4, null, "Hồ sơ không phù hợp. Ứng viên làm mảng Marketing, hoàn toàn không có kỹ năng lập trình phần mềm.", 10, null, null, null, null, null, null, new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, "Tìm kiếm cơ hội mới.", null, null, 1, 4, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CandidateId",
                table: "Applications",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobPostingId",
                table: "Applications",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ResumeId",
                table: "Applications",
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
                name: "IX_CvBankEntries_FolderId",
                table: "CvBankEntries",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_CvBankEntries_RecruiterId",
                table: "CvBankEntries",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_CvFolders_RecruiterId",
                table: "CvFolders",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifications_UserId",
                table: "EmailVerifications",
                column: "UserId");

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
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CvBankEntries");

            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropTable(
                name: "RecruiterRequests");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "JobPostings");

            migrationBuilder.DropTable(
                name: "Resumes");

            migrationBuilder.DropTable(
                name: "CvFolders");

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
