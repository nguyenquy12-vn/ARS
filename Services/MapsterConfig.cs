using Mapster;
using Domain.Entities;
using Services.DTOs.Auth;
using Services.DTOs.User;
using Services.DTOs.Application;
using Services.DTOs.JobPosting;


namespace Services;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserAuthResponse>()
            .Map(dest => dest.RoleName, src => src.Role != null ? src.Role.Name : string.Empty);
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.RoleName, src => src.Role != null ? src.Role.Name : "Error")
            .Map(dest => dest.DisplayedRoleName, src => src.Role != null ? src.Role.DisplayedName : "Error")
            .Map(dest => dest.Status, src => src.Status.ToString());
        config.NewConfig<Application, ApplicationDto>()
            .Map(dest => dest.JobTitle, src => src.JobPosting != null ? src.JobPosting.Title : "Error")
            .Map(dest => dest.CompanyName, src => src.JobPosting != null && src.JobPosting.Company != null ? src.JobPosting.Company.CompanyName : "Error")
            .Map(dest => dest.CompanyLogoPath, src => src.JobPosting != null && src.JobPosting.Company != null ? src.JobPosting.Company.LogoPath : "Error");
        config.NewConfig<Application, CandidateApplicationDto>()
            .Map(dest => dest.JobTitle, src => src.JobPosting != null ? src.JobPosting.Title : "Error");
        config.NewConfig<Application, JobApplicationDto>()
            .Map(dest => dest.CandidateName, src => src.Candidate != null ? src.Candidate.FullName : "Error");
        config.NewConfig<JobPosting, JobListItem>()
            .Map(dest => dest.JobCategoryName, src => src.JobCategory != null ? src.JobCategory.Name : "Error")
            .Map(dest => dest.ApplicationsCount, src => src.Applications != null ? src.Applications.Count : 0)
            .Map(dest => dest.Status, src => src.Status.ToString());
        config.NewConfig<JobPosting, JobDto>()
            .Map(dest => dest.CompanyName, src => src.Company != null ? src.Company.CompanyName : "Error")
            .Map(dest => dest.RecruiterId, src => src.Company != null ? src.Company.RecruiterId : 0)
            .Map(dest => dest.JobCategoryName, src => src.JobCategory != null ? src.JobCategory.Name : "Error")
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.WorkMode, src => src.WorkMode.ToString())
            .Map(dest => dest.JobType, src => src.JobType.ToString());

    }
}