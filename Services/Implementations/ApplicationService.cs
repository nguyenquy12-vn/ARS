using Infrastructure;
using Services.Interfaces;

namespace Services.Implementations;

public class ApplicationService : IApplicationService
{
    private readonly ARSDbContext _context;

    public ApplicationService(ARSDbContext context)
    {
        _context = context;
    }


}
