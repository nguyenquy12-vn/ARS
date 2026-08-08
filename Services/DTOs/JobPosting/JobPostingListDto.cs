namespace Services.DTOs.JobPosting;

public class JobPostingListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoPath { get; set; }
    public string Location { get; set; } = string.Empty;
    public string JobTypeName { get; set; } = string.Empty;
    public string WorkModeName { get; set; } = string.Empty;
    public int? MinSalary { get; set; }
    public int? MaxSalary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
