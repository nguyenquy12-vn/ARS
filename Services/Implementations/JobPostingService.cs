using Infrastructure;
using Services.Interfaces;

namespace Services.Implementations;

public class JobPostingService : IJobPostingService
{
    private readonly ARSDbContext _context;

    public JobPostingService(ARSDbContext context)
    {
        _context = context;
    }
}
