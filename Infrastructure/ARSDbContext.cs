using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ARSDbContext : DbContext
{
    public ARSDbContext(DbContextOptions<ARSDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RecruiterRequest> RecruiterRequests { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<JobCategory> JobCategories { get; set; }
    public DbSet<Resume> Resumes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enum string conversions
        modelBuilder.Entity<User>()
            .Property(u => u.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Role>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<RecruiterRequest>()
            .Property(r => r.Status)
            .HasConversion<string>();

        // Composite key configuration
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Unique indexes
        modelBuilder.Entity<Company>()
            .HasIndex(c => c.RecruiterId)
            .IsUnique();

        modelBuilder.Entity<Company>()
            .HasIndex(c => c.TaxCode)
            .IsUnique();

        // Prevent multiple cascade paths
        modelBuilder.Entity<Application>()
            .HasOne(a => a.Resume)
            .WithMany(r => r.Applications)
            .HasForeignKey(a => a.ResumeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Application>()
            .HasOne(a => a.Candidate)
            .WithMany(u => u.Applications)
            .HasForeignKey(a => a.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed: Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", DisplayedName = "Quản trị viên", Description = "Quản trị viên toàn quyền hệ thống" },
            new Role { Id = 2, Name = "Recruiter", DisplayedName = "Nhà tuyển dụng", Description = "Nhà tuyển dụng (Đăng tin, duyệt CV, dùng AI)" },
            new Role { Id = 3, Name = "Candidate", DisplayedName = "Ứng viên", Description = "Ứng viên (Tìm việc, nộp CV)" }
        );

        // Seed: JobCategories
        modelBuilder.Entity<JobCategory>().HasData(
            new JobCategory { Id = 1, Name = "Công nghệ thông tin / Phần mềm", Description = "Lập trình viên, Tester, AI Engineer, DevOps..." },
            new JobCategory { Id = 2, Name = "Kinh doanh / Bán hàng", Description = "Sales Executive, Account Manager, Chăm sóc khách hàng..." },
            new JobCategory { Id = 3, Name = "Marketing / Truyền thông", Description = "Digital Marketing, Content Creator, SEO, Event Organizer..." },
            new JobCategory { Id = 4, Name = "Tài chính / Kế toán", Description = "Kế toán tổng hợp, Kiểm toán viên, Phân tích tài chính..." },
            new JobCategory { Id = 5, Name = "Hành chính / Nhân sự", Description = "Tuyển dụng, C&B, Trợ lý, Quản lý văn phòng..." },
            new JobCategory { Id = 6, Name = "Thiết kế / Đồ họa", Description = "UI/UX Designer, Graphic Designer, Video Editor..." },
            new JobCategory { Id = 7, Name = "Biên phiên dịch / Ngôn ngữ", Description = "Phiên dịch viên Tiếng Anh, Tiếng Trung, Tiếng Nhật..." }
        );

        // Seed: Permissions from Enum
        var permissions = Enum.GetValues(typeof(PermissionType))
            .Cast<PermissionType>()
            .Select(p => new Permission
            {
                Id = (int)p,
                Name = p.ToString(),
                Description = GetPermissionDescription(p)
            })
            .ToArray();

        modelBuilder.Entity<Permission>().HasData(permissions);

        // Seed: RolePermissions
        modelBuilder.Entity<RolePermission>().HasData(
            // Admin: ManageRoles, ManageUsers
            new RolePermission { RoleId = 1, PermissionId = (int)PermissionType.ManageRoles },
            new RolePermission { RoleId = 1, PermissionId = (int)PermissionType.ManageUsers },

            // Recruiter: ViewJob, CreateJob, EditJob, DeleteJob, ReviewCV, EvaluateAI
            new RolePermission { RoleId = 2, PermissionId = (int)PermissionType.ViewJob },
            new RolePermission { RoleId = 2, PermissionId = (int)PermissionType.CreateJob },
            new RolePermission { RoleId = 2, PermissionId = (int)PermissionType.EditJob },
            new RolePermission { RoleId = 2, PermissionId = (int)PermissionType.DeleteJob },
            new RolePermission { RoleId = 2, PermissionId = (int)PermissionType.ReviewCV },
            new RolePermission { RoleId = 2, PermissionId = (int)PermissionType.EvaluateAI },

            // Candidate: ViewJob, ApplyJob
            new RolePermission { RoleId = 3, PermissionId = (int)PermissionType.ViewJob },
            new RolePermission { RoleId = 3, PermissionId = (int)PermissionType.ApplyJob }
        );

        // BCrypt hash of "Password123@"
        string defaultPasswordHash = "$2a$12$AZ2uR7y2CIwawpCEIjfrBOUtjC5PSpDFH5gWJn2Y0bgJUGGBkYvuy";

        // Seed: Users
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, RoleId = 1, FullName = "Hệ Thống Admin", Email = "admin@ars.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0123456789", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 2, RoleId = 2, FullName = "Nguyễn Văn Tuyển FPT", Email = "recruiter1@fpt.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0987654321", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 3, RoleId = 2, FullName = "Trần Thị Duyệt Viettel", Email = "recruiter2@viettel.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0912345678", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 4, RoleId = 3, FullName = "Lê Văn Pro .NET", Email = "candidate1@gmail.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0333444555", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 5, RoleId = 3, FullName = "Nguyễn Thị Fresher", Email = "candidate2@gmail.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0333444666", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 6, RoleId = 3, FullName = "Trần Văn Intern", Email = "candidate3@gmail.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0333444777", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 7, RoleId = 3, FullName = "Hoàng Lệ Trái Ngành", Email = "candidate4@gmail.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0333444888", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed: Companies
        modelBuilder.Entity<Company>().HasData(
            new Company
            {
                Id = 1,
                RecruiterId = 2,
                CompanyName = "FPT Software",
                TaxCode = "0101248141",
                Address = "Khu Công nghệ cao Hòa Lạc, Thạch Thất, Hà Nội",
                CompanySize = "10000+ nhân viên",
                Overview = "Tập đoàn công nghệ hàng đầu Việt Nam.",
                Website = "https://fpt-software.com",
                LogoPath = "/uploads/logos/fpt-software.png"
            }
        );

        // Seed: JobPostings
        modelBuilder.Entity<JobPosting>().HasData(
            new JobPosting
            {
                Id = 1,
                CompanyId = 1,
                Title = "Kỹ Sư Lập Trình Backend .NET",
                Description = "Phát triển các hệ thống Web API quy mô lớn sử dụng .NET 8 và SQL Server.",
                Requirements = "Có kinh nghiệm lập trình C#, hiểu biết về Entity Framework Core, SQL Server. Biết Docker là một lợi thế.",
                Benefits = "Lương thưởng tháng 13, bảo hiểm FPT Care, làm việc hybrid.",
                Location = "Cầu Giấy, Hà Nội",
                JobType = JobType.FullTime,
                WorkMode = WorkMode.Hybrid,
                JobCategoryId = 1,
                MinSalary = 15000000,
                MaxSalary = 25000000,
                Status = JobStatus.Active,
                Vacancies = 3,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ExpiredAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new JobPosting
            {
                Id = 2,
                CompanyId = 1,
                Title = "Chuyên viên Marketing Digital",
                Description = "Lên kế hoạch và triển khai các chiến dịch quảng cáo trên nền tảng Digital (Facebook, Google).",
                Requirements = "Tối thiểu 1 năm kinh nghiệm chạy Ads. Có khả năng sáng tạo nội dung.",
                Benefits = "Thưởng theo KPIs, môi trường trẻ trung năng động.",
                Location = "Thanh Xuân, Hà Nội",
                JobType = JobType.FullTime,
                WorkMode = WorkMode.Onsite,
                JobCategoryId = 3,
                MinSalary = 10000000,
                MaxSalary = 20000000,
                Status = JobStatus.Active,
                Vacancies = 2,
                CreatedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                ExpiredAt = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc)
            },
            new JobPosting
            {
                Id = 3,
                CompanyId = 1,
                Title = "Nhân viên Telesales",
                Description = "Tư vấn sản phẩm dịch vụ của công ty đến với khách hàng qua điện thoại.",
                Requirements = "Giọng nói chuẩn, giao tiếp tốt, không yêu cầu kinh nghiệm (được đào tạo).",
                Benefits = "Hoa hồng cao, phụ cấp ăn trưa và đi lại.",
                Location = "Đống Đa, Hà Nội",
                JobType = JobType.FullTime,
                WorkMode = WorkMode.Onsite,
                JobCategoryId = 2,
                MinSalary = 7000000,
                MaxSalary = 15000000,
                Status = JobStatus.Active,
                Vacancies = 5,
                CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                ExpiredAt = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new JobPosting
            {
                Id = 4,
                CompanyId = 1,
                Title = "Thực tập sinh ReactJS",
                Description = "Tham gia phát triển các tính năng Front-end cho dự án công ty bằng ReactJS.",
                Requirements = "Sinh viên năm cuối hoặc mới ra trường, nắm chắc HTML/CSS/JS cơ bản.",
                Benefits = "Hỗ trợ dấu mộc thực tập, trợ cấp 3 triệu/tháng, cơ hội lên nhân viên chính thức.",
                Location = "Quận 1, TP. HCM",
                JobType = JobType.Internship,
                WorkMode = WorkMode.Remote,
                JobCategoryId = 1,
                MinSalary = 3000000,
                MaxSalary = 5000000,
                Status = JobStatus.Active,
                Vacancies = 4,
                CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                ExpiredAt = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new JobPosting
            {
                Id = 5,
                CompanyId = 1,
                Title = "Kế toán tổng hợp",
                Description = "Chịu trách nhiệm kiểm tra đối chiếu số liệu, lập báo cáo tài chính hàng tháng/quý.",
                Requirements = "Tốt nghiệp đại học chuyên ngành Kế toán, trên 3 năm kinh nghiệm làm tổng hợp.",
                Benefits = "Chế độ BHYT, BHXH đầy đủ, thưởng lễ tết hấp dẫn.",
                Location = "Hải Châu, Đà Nẵng",
                JobType = JobType.FullTime,
                WorkMode = WorkMode.Onsite,
                JobCategoryId = 4,
                MinSalary = 12000000,
                MaxSalary = 18000000,
                Status = JobStatus.Active,
                Vacancies = 1,
                CreatedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                ExpiredAt = new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed: Resumes
        modelBuilder.Entity<Resume>().HasData(
            new Resume { Id = 1, CandidateId = 4, Title = "CV Lê Văn Pro - .NET Developer", FilePath = "/uploads/cv1.pdf", IsDefault = true, RawTextContent = "Kinh nghiệm 3 năm làm việc với C#, chuyên sâu Web API, Entity Framework Core, SQL Server và Docker.", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Resume { Id = 2, CandidateId = 5, Title = "CV Nguyễn Thị Fresher", FilePath = "/uploads/cv2.pdf", IsDefault = true, RawTextContent = "Sinh viên mới tốt nghiệp, biết cơ bản về C# và OOP, chưa có kinh nghiệm thực tế hệ thống lớn.", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Resume { Id = 3, CandidateId = 6, Title = "CV Trần Văn Intern Backend", FilePath = "/uploads/cv3.pdf", IsDefault = true, RawTextContent = "Sinh viên năm 4 tìm kiếm vị trí thực tập, biết viết câu lệnh SQL cơ bản, đang học C#.", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Resume { Id = 4, CandidateId = 7, Title = "CV Hoàng Lệ - Sales Marketing", FilePath = "/uploads/cv4.pdf", IsDefault = true, RawTextContent = "Kinh nghiệm 2 năm chạy quảng cáo Facebook, Google Ads, tư vấn chốt đơn hàng.", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed: Applications
        modelBuilder.Entity<Application>().HasData(
            new Application { Id = 1, JobPostingId = 1, CandidateId = 4, ResumeId = 1, CoverLetter = "Tôi rất mong muốn được làm việc tại FPT.", Status = ApplicationStatus.Reviewing, AiMatchScore = 95, AiFeedback = "Hồ sơ hoàn hảo. Ứng viên có đầy đủ kỹ năng cứng về C#, EF Core, SQL và Docker trùng khớp hoàn toàn với JD.", AppliedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Application { Id = 2, JobPostingId = 1, CandidateId = 5, ResumeId = 2, CoverLetter = "Mong công ty cho cơ hội phỏng vấn.", Status = ApplicationStatus.Reviewing, AiMatchScore = 65, AiFeedback = "Ứng viên có kiến thức nền tảng C# nhưng thiếu kinh nghiệm thực tế với SQL và hệ thống lớn theo yêu cầu.", AppliedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Application { Id = 3, JobPostingId = 1, CandidateId = 6, ResumeId = 3, CoverLetter = "Xin thực tập ạ.", Status = ApplicationStatus.Reviewing, AiMatchScore = 40, AiFeedback = "Hồ sơ còn khá yếu, chưa đáp ứng được các tiêu chí kỹ thuật tối thiểu của vị trí hiện tại.", AppliedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
            new Application { Id = 4, JobPostingId = 1, CandidateId = 7, ResumeId = 4, CoverLetter = "Tìm kiếm cơ hội mới.", Status = ApplicationStatus.Reviewing, AiMatchScore = 10, AiFeedback = "Hồ sơ không phù hợp. Ứng viên làm mảng Marketing, hoàn toàn không có kỹ năng lập trình phần mềm.", AppliedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc) }
        );
    }

    private string GetPermissionDescription(PermissionType permission)
    {
        return permission switch
        {
            PermissionType.ViewJob => "Xem danh sách tin tuyển dụng",
            PermissionType.CreateJob => "Tạo mới tin tuyển dụng",
            PermissionType.EditJob => "Chỉnh sửa tin tuyển dụng",
            PermissionType.DeleteJob => "Xóa tin tuyển dụng",
            PermissionType.ApplyJob => "Nộp hồ sơ ứng tuyển (CV)",
            PermissionType.ReviewCV => "Xem và đánh giá hồ sơ ứng viên",
            PermissionType.EvaluateAI => "Sử dụng trí tuệ nhân tạo (AI) để chấm điểm CV",
            PermissionType.ManageRoles => "Quản lý vai trò và phân quyền hệ thống",
            PermissionType.ManageUsers => "Quản lý danh sách người dùng",
            _ => permission.ToString()
        };
    }
}