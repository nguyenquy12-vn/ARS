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
            new Role { Id = 1, Name = "Admin", Description = "Quản trị viên toàn quyền hệ thống" },
            new Role { Id = 2, Name = "Recruiter", Description = "Nhà tuyển dụng (Đăng tin, duyệt CV, dùng AI)" },
            new Role { Id = 3, Name = "Candidate", Description = "Ứng viên (Tìm việc, nộp CV)" }
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

        // BCrypt hash of "Password123@"
        string defaultPasswordHash = "$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.";

        // Seed: Users
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, RoleId = 1, FullName = "Hệ Thống Admin", Email = "admin@ars.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0123456789", Status = UserStatus.Active, CreatedAt = DateTime.UtcNow },
            new User { Id = 2, RoleId = 2, FullName = "Nguyễn Văn Tuyển FPT", Email = "recruiter1@fpt.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0987654321", Status = UserStatus.Active, CreatedAt = DateTime.UtcNow },
            new User { Id = 3, RoleId = 2, FullName = "Trần Thị Duyệt Viettel", Email = "recruiter2@viettel.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0912345678", Status = UserStatus.Active, CreatedAt = DateTime.UtcNow },
            new User { Id = 4, RoleId = 3, FullName = "Lê Văn Pro .NET", Email = "candidate1@gmail.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0333444555", Status = UserStatus.Active, CreatedAt = DateTime.UtcNow },
            new User { Id = 5, RoleId = 3, FullName = "Nguyễn Thị Fresher", Email = "candidate2@gmail.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0333444666", Status = UserStatus.Active, CreatedAt = DateTime.UtcNow },
            new User { Id = 6, RoleId = 3, FullName = "Trần Văn Intern", Email = "candidate3@gmail.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0333444777", Status = UserStatus.Active, CreatedAt = DateTime.UtcNow },
            new User { Id = 7, RoleId = 3, FullName = "Hoàng Lệ Trái Ngành", Email = "candidate4@gmail.com", PasswordHash = defaultPasswordHash, PhoneNumber = "0333444888", Status = UserStatus.Active, CreatedAt = DateTime.UtcNow }
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
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(30)
            }
        );

        // Seed: Resumes
        modelBuilder.Entity<Resume>().HasData(
            new Resume { Id = 1, CandidateId = 4, Title = "CV Lê Văn Pro - .NET Developer", FilePath = "/uploads/cv1.pdf", IsDefault = true, RawTextContent = "Kinh nghiệm 3 năm làm việc với C#, chuyên sâu Web API, Entity Framework Core, SQL Server và Docker.", CreatedAt = DateTime.UtcNow },
            new Resume { Id = 2, CandidateId = 5, Title = "CV Nguyễn Thị Fresher", FilePath = "/uploads/cv2.pdf", IsDefault = true, RawTextContent = "Sinh viên mới tốt nghiệp, biết cơ bản về C# và OOP, chưa có kinh nghiệm thực tế hệ thống lớn.", CreatedAt = DateTime.UtcNow },
            new Resume { Id = 3, CandidateId = 6, Title = "CV Trần Văn Intern Backend", FilePath = "/uploads/cv3.pdf", IsDefault = true, RawTextContent = "Sinh viên năm 4 tìm kiếm vị trí thực tập, biết viết câu lệnh SQL cơ bản, đang học C#.", CreatedAt = DateTime.UtcNow },
            new Resume { Id = 4, CandidateId = 7, Title = "CV Hoàng Lệ - Sales Marketing", FilePath = "/uploads/cv4.pdf", IsDefault = true, RawTextContent = "Kinh nghiệm 2 năm chạy quảng cáo Facebook, Google Ads, tư vấn chốt đơn hàng.", CreatedAt = DateTime.UtcNow }
        );

        // Seed: Applications
        modelBuilder.Entity<Application>().HasData(
            new Application { Id = 1, JobPostingId = 1, CandidateId = 4, ResumeId = 1, CoverLetter = "Tôi rất mong muốn được làm việc tại FPT.", Status = ApplicationStatus.Reviewing, AiMatchScore = 95, AiFeedback = "Hồ sơ hoàn hảo. Ứng viên có đầy đủ kỹ năng cứng về C#, EF Core, SQL và Docker trùng khớp hoàn toàn với JD.", AppliedAt = DateTime.UtcNow },
            new Application { Id = 2, JobPostingId = 1, CandidateId = 5, ResumeId = 2, CoverLetter = "Mong công ty cho cơ hội phỏng vấn.", Status = ApplicationStatus.Reviewing, AiMatchScore = 65, AiFeedback = "Ứng viên có kiến thức nền tảng C# nhưng thiếu kinh nghiệm thực tế với SQL và hệ thống lớn theo yêu cầu.", AppliedAt = DateTime.UtcNow },
            new Application { Id = 3, JobPostingId = 1, CandidateId = 6, ResumeId = 3, CoverLetter = "Xin thực tập ạ.", Status = ApplicationStatus.Reviewing, AiMatchScore = 40, AiFeedback = "Hồ sơ còn khá yếu, chưa đáp ứng được các tiêu chí kỹ thuật tối thiểu của vị trí hiện tại.", AppliedAt = DateTime.UtcNow },
            new Application { Id = 4, JobPostingId = 1, CandidateId = 7, ResumeId = 4, CoverLetter = "Tìm kiếm cơ hội mới.", Status = ApplicationStatus.Reviewing, AiMatchScore = 10, AiFeedback = "Hồ sơ không phù hợp. Ứng viên làm mảng Marketing, hoàn toàn không có kỹ năng lập trình phần mềm.", AppliedAt = DateTime.UtcNow }
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