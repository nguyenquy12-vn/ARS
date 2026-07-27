using Domain.Enums;
using Infrastructure;
using MapsterMapper;
using Services.Interfaces;
using Services.DTOs.User;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Domain.Constraints;
using Services.DTOs.Application;
using Services.DTOs.Company;

namespace Services.Implementations;

public class UserService : IUserService
{
    private readonly ARSDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(ARSDbContext context, IMapper mapper, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<(List<UserDto> Users, int TotalCount)> GetUserListAsync(string? search, string? role, string? status, int page, int pageSize)
    {
        try
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u => u.FullName.Contains(s) || u.Email.Contains(s) || (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role != null && u.Role.Name == role);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<Domain.Enums.UserStatus>(status, true, out var st))
                {
                    query = query.Where(u => u.Status == st);
                }
            }

            var total = await query.CountAsync();

            // Ensure sensible paging
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 200);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (_mapper.Map<List<UserDto>>(users), total);
        }
        catch (System.Exception ex)
        {
            _logger?.LogError(ex, "Error fetching user list with search={Search} role={Role} status={Status}", search, role, status);
            return (new List<UserDto>(), 0);
        }
    }

    public async Task<bool> IsUserLockedAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }
            return user.Status == UserStatus.Locked;
        }
        catch (System.Exception ex)
        {
            _logger?.LogWarning(ex, "Error checking user lock status for {UserId}, defaulting to unlocked.", userId);
            return false;
        }
    }

    public async Task<UserDetailsResponse> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return UserDetailsResponse.Failure(ErrorMessage.UserNotFound);
            }

            List<ResumeDto>? resumes = null;
            CompanyProfileDto companyProfile = null;

            if (user.Role != null)
            {
                if (user.Role.Name == "Candidate")
                {
                    resumes = await _context.Resumes
                        .Include(r => r.Applications)
                        .ThenInclude(a => a.JobPosting)
                        .Where(r => r.CandidateId == user.Id)
                        .Select(r => _mapper.Map<ResumeDto>(r))
                        .ToListAsync();
                }
                else if (user.Role.Name == "Recruiter")
                {
                    var company = await _context.Companies.FirstOrDefaultAsync(c => c.RecruiterId == user.Id);
                    if (company != null)
                    {
                        companyProfile = _mapper.Map<CompanyProfileDto>(company);
                    }
                }
            }

            return UserDetailsResponse.Success(_mapper.Map<UserDto>(user), resumes, companyProfile);
        }
        catch (System.Exception ex)
        {
            _logger?.LogError(ex, "Error fetching user details for {UserId}", userId);
            return UserDetailsResponse.Failure("Could not load user details at this time.");
        }
    }
}
