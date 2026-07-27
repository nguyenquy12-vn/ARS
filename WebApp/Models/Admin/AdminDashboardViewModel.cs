namespace WebApp.Models.Admin;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalJobs { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalApplications { get; set; }

    public List<RecentUser> RecentUsers { get; set; } = new();
    public List<RecentApplication> RecentApplications { get; set; } = new();
}

public record RecentUser
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record RecentApplication
{
    public int Id { get; init; }
    public int CandidateId { get; init; }
    public int JobPostingId { get; init; }
    public DateTime AppliedAt { get; init; }
}
