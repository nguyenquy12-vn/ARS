using Domain.Constraints;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.Auth;
using Services.Interfaces;

namespace Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ARSDbContext _context;
    private readonly IMapper _mapper;

    public AuthService(ARSDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // 1. Tìm kiếm User theo Email
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return LoginResponse.Failure(ErrorMessage.InvalidLogin);
        }

        // 2. Kiểm tra mật khẩu
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return LoginResponse.Failure(ErrorMessage.InvalidLogin);
        }

        if (user.Status == UserStatus.Locked)
        {
            return LoginResponse.Failure(ErrorMessage.AccountLocked);
        }

        var userResponse = _mapper.Map<UserAuthResponse>(user);

        var permissions = await _context.RolePermissions
            .Where(p => p.RoleId == user.RoleId)
            .Select(p => p.Permission != null ? p.Permission.Name : "Error")
            .ToListAsync();


        return LoginResponse.Success(userResponse, permissions);
    }

    public async Task<BoolResponse> RegisterCandidateAsync(RegisterRequest request)
    {
        var isEmailExist = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (isEmailExist)
        {
            return BoolResponse.Failure(ErrorMessage.DuplicateEmail);
        }

        var candidateRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate");
        if (candidateRole == null)
        {
            return BoolResponse.Failure(ErrorMessage.CandidateRoleNotAvailable);
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash, 
            FullName = request.FullName,
            RoleId = candidateRole.Id, 
        };

        try
        {
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return BoolResponse.Success();
        }
        catch (Exception ex)
        {
            return BoolResponse.Failure(ErrorMessage.ExceptionError);
        }
    }

    public async Task<BoolResponse> LockAccountAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return BoolResponse.Failure(ErrorMessage.UserNotFound);
        }

        user.Status = UserStatus.Locked;
        _context.Users.Update(user);

        await _context.SaveChangesAsync();

        return BoolResponse.Success();

    }
    public async Task<BoolResponse> UnlockAccountAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return BoolResponse.Failure(ErrorMessage.UserNotFound);
        }

        user.Status = UserStatus.Active;
        _context.Users.Update(user);

        await _context.SaveChangesAsync();

        return BoolResponse.Success();

    }

}