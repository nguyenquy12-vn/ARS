using Domain.Constraints;
using Domain.Entities;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.Company;
using Services.Interfaces;

namespace Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly ARSDbContext _context;

    public CompanyService(ARSDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyProfileDto?> GetByRecruiterAsync(int recruiterId)
    {
        return await _context.Companies
            .Where(c => c.RecruiterId == recruiterId)
            .Select(c => new CompanyProfileDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                TaxCode = c.TaxCode,
                Address = c.Address,
                LogoPath = c.LogoPath,
                CompanySize = c.CompanySize,
                Overview = c.Overview,
                Website = c.Website
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CompanyResult> SaveAsync(int recruiterId, CompanyFormRequest request)
    {
        var taxCode = request.TaxCode.Trim();

        // Mã số thuế phải là duy nhất (không trùng với công ty của recruiter khác)
        var taxCodeTaken = await _context.Companies
            .AnyAsync(c => c.TaxCode == taxCode && c.RecruiterId != recruiterId);
        if (taxCodeTaken)
        {
            return CompanyResult.Failure(ErrorMessage.DuplicateTaxCode);
        }

        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.RecruiterId == recruiterId);

        var isNew = company == null;
        if (company == null)
        {
            company = new Company { RecruiterId = recruiterId };
            _context.Companies.Add(company);
        }

        company.CompanyName = request.CompanyName.Trim();
        company.TaxCode = taxCode;
        company.Address = request.Address?.Trim();
        company.LogoPath = request.LogoPath?.Trim();
        company.CompanySize = request.CompanySize?.Trim();
        company.Overview = request.Overview;
        company.Website = request.Website?.Trim();

        try
        {
            await _context.SaveChangesAsync();
            return CompanyResult.Success(company.Id);
        }
        catch (Exception)
        {
            return CompanyResult.Failure(ErrorMessage.CompanySaveError);
        }
    }
}
