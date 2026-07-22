using Domain.Entities;
using Services.DTOs.Application;
using Services.DTOs.Company;

namespace Services.DTOs.User;

public class UserDetailsResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public List<ResumeDto>? Resumes { get; set; } = new List<ResumeDto>();
    public CompanyProfileDto? CompanyProfile { get; set; }

    public static UserDetailsResponse Success(UserDto user, List<ResumeDto>? resumes, CompanyProfileDto? companyProfile) => new() { IsSuccess = true, User = user, Resumes = resumes, CompanyProfile = companyProfile };

    public static UserDetailsResponse Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}

