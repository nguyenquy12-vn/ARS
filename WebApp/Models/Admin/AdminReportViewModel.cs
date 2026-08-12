namespace WebApp.Models.Admin;

public class AdminReportViewModel
{
    public int TotalUsers { get; set; }
    public int TotalJobs { get; set; }
    public int ActiveJobs { get; set; }
    public int TotalApplications { get; set; }
    public int SuccessfulApplications { get; set; }
    public decimal TotalRevenue { get; set; }
    public double ConversionRate { get; set; }
    public double AverageAiScore { get; set; }
    public List<ReportPoint> UsersByRole { get; set; } = [];
    public List<ReportPoint> JobsByCategory { get; set; } = [];
    public List<ReportPoint> ApplicationsByStatus { get; set; } = [];
    public List<ReportPoint> MonthlyApplications { get; set; } = [];
    public List<ReportPoint> MonthlyUsers { get; set; } = [];
    public List<TopJobReport> TopJobs { get; set; } = [];
}

public record ReportPoint(string Label, int Value);
public record TopJobReport(string Title, string Company, int Applications, double AverageScore, int Accepted);
