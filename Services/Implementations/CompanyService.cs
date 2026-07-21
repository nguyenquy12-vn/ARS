using Infrastructure;
using Services.Interfaces;

namespace Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly ARSDbContext _context;

    public CompanyService(ARSDbContext context)
    {
        _context = context;
    }
}
