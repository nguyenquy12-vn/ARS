using Services.DTOs.Application;
using Services.DTOs.JobPosting;

namespace WebApp.Models.Application;

public class JobApplicantsViewModel
{
    public JobPostingDetail Job { get; set; } = new();
    public List<ApplicantListItem> Applicants { get; set; } = new();
}
