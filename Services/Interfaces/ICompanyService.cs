using Services.DTOs.Company;

namespace Services.Interfaces;

public interface ICompanyService
{
    Task<CompanyProfileDto?> GetByRecruiterAsync(int recruiterId);

    Task<CompanyResult> SaveAsync(int recruiterId, CompanyFormRequest request);
}
