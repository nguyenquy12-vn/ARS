namespace Services.DTOs.JobPosting;

public class JobListItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string JobCategoryName { get; set; } = string.Empty;

    public int ApplicationsCount { get; set; } = 0;

    public string Status { get; set; } = string.Empty;      // Draft, Active, Closed, Archived

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiredAt { get; set; }
}
