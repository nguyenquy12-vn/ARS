using Domain.Enums;
using Infrastructure;
using MapsterMapper;
using Services.Interfaces;
using Services.DTOs.User;
using Microsoft.EntityFrameworkCore;
using Domain.Constraints;
using Services.DTOs.Application;
using Services.DTOs.Company;

namespace Services.Implementations;

public class UserService : IUserService
{
    private readonly ARSDbContext _context;
    private readonly IMapper _mapper;

    public UserService(ARSDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> GetUserListAsync()
    {
        var users = await _context.Users.Include(u => u.Role).ToListAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<bool> IsUserLockedAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        return user.Status == UserStatus.Locked;
    }

    public async Task<UserDetailsResponse> GetUserByIdAsync(int userId)
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
}
