using Services.DTOs.Application;
using Services.DTOs.Company;
using Services.DTOs.User;

namespace WebApp.Models.Admin;

public class UserDetailsViewModel
{
    public UserDto? User { get; set; } = new();
    public List<ResumeDto>? Resumes { get; set; } = new();
    public CompanyProfileDto? CompanyProfile { get; set; } = new();
}
