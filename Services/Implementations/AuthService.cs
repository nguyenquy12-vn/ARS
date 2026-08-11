using Domain.Constraints;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Services.DTOs.Auth;
using Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ARSDbContext _context;
    private readonly IMapper _mapper;
    private readonly Services.Interfaces.IEmailService _emailService;
    private readonly Microsoft.Extensions.Logging.ILogger<AuthService> _logger;
    private readonly IMemoryCache _cache;
    private const int ResendCooldownSeconds = 60; // cooldown duration in seconds
    private const int MaxOtpAttempts = 5;
    private static readonly TimeSpan OtpAttemptWindow = TimeSpan.FromMinutes(15);

    public AuthService(ARSDbContext context, IMapper mapper, Services.Interfaces.IEmailService emailService, Microsoft.Extensions.Logging.ILogger<AuthService> logger, IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _emailService = emailService;
        _logger = logger;
        _cache = cache;
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

        if (!user.IsEmailVerified)
        {
            // Allow legacy/existing accounts (created before EmailVerification was enforced)
            // to sign in without OTP if they don't have any EmailVerification records.
            var hasVerificationRecord = await _context.EmailVerifications.AnyAsync(e => e.UserId == user.Id);
            if (!hasVerificationRecord)
            {
                // mark as verified to avoid asking again
                user.IsEmailVerified = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                return LoginResponse.Failure(ErrorMessage.EmailNotVerified);
            }
        }

        var userResponse = _mapper.Map<UserAuthResponse>(user);

        var permissions = await _context.RolePermissions
            .Where(p => p.RoleId == user.RoleId)
            .Select(p => p.Permission != null ? p.Permission.Name : "Error")
            .ToListAsync();


        return LoginResponse.Success(userResponse, permissions);
    }

    public async Task<BoolResponse> ResendEmailOtpAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return BoolResponse.Failure(ErrorMessage.UserNotFound);
        // Check cooldown: find the latest verification created for this user
        var last = await _context.EmailVerifications
            .Where(e => e.UserId == user.Id)
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync();

        if (last != null)
        {
            var secondsSince = (DateTime.UtcNow - last.ExpiresAt.AddMinutes(-15)).TotalSeconds; // created at ExpiresAt-15min
            // alternatively compute created time by subtracting TTL
            var createdAt = last.ExpiresAt.AddMinutes(-15);
            var secondsSinceCreated = (DateTime.UtcNow - createdAt).TotalSeconds;
            var remaining = ResendCooldownSeconds - (int)secondsSinceCreated;
            if (remaining > 0)
            {
                return BoolResponse.Failure($"Vui lòng chờ {remaining} giây trước khi gửi lại mã.");
            }
        }

        // Create new OTP
        var code = GenerateOtpCode();
        var ev = new Domain.Entities.EmailVerification
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };

        await _context.EmailVerifications.AddAsync(ev);
        await _context.SaveChangesAsync();

        try
        {
            var body = $"<p>Your verification code is: <strong>{code}</strong></p><p>It will expire in 15 minutes.</p>";
            await _emailService.SendEmailAsync(user.Email, "Verify your email", body);
            _logger.LogInformation("Sent verification email to {Email} (userId={UserId})", user.Email, user.Id);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            return BoolResponse.Failure("Không thể gửi email xác thực. Vui lòng thử lại sau hoặc liên hệ quản trị hệ thống.");
        }

        return BoolResponse.Success();
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
            IsEmailVerified = false,
        };

        try
        {
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            // Generate OTP and store
            var code = GenerateOtpCode();
            var ev = new Domain.Entities.EmailVerification
            {
                UserId = newUser.Id,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            await _context.EmailVerifications.AddAsync(ev);
            await _context.SaveChangesAsync();

            // Send email
            try
            {
                var body = $"<p>Your verification code is: <strong>{code}</strong></p><p>It will expire in 15 minutes.</p>";
                await _emailService.SendEmailAsync(newUser.Email, "Verify your email", body);
                _logger.LogInformation("Sent verification email to {Email} (new user id={UserId})", newUser.Email, newUser.Id);
                return BoolResponse.Success();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email} after registration", newUser.Email);
                // Keep user created but inform caller that email delivery failed so UI can instruct user to resend or contact support
                return new BoolResponse { IsSuccess = true, ErrorMessage = "Không thể gửi email xác thực. Mã có thể chưa tới hộp thư của bạn. Vui lòng thử gửi lại mã hoặc liên hệ quản trị." };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register candidate with email {Email}", request.Email);
            return BoolResponse.Failure(ErrorMessage.ExceptionError);
        }
    }

    public async Task<BoolResponse> VerifyEmailOtpAsync(string email, string code)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return BoolResponse.Failure(ErrorMessage.UserNotFound);

        var attemptKey = GetOtpAttemptKey(user.Id);
        var attempts = _cache.Get<int>(attemptKey);
        if (attempts >= MaxOtpAttempts)
        {
            return BoolResponse.Failure("Bạn đã nhập sai OTP quá nhiều lần. Vui lòng gửi lại mã mới hoặc thử lại sau.");
        }

        var ev = await _context.EmailVerifications
            .Where(e => e.UserId == user.Id && !e.IsUsed && e.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync();

        if (ev == null) return BoolResponse.Failure("Verification code expired or not found");

        if (ev.Code != code)
        {
            _cache.Set(attemptKey, attempts + 1, OtpAttemptWindow);
            return BoolResponse.Failure("Invalid verification code");
        }

        ev.IsUsed = true;
        user.IsEmailVerified = true;
        _context.EmailVerifications.Update(ev);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        _cache.Remove(attemptKey);

        return BoolResponse.Success();
    }

    public async Task<UserAuthResponse?> GetOrCreateExternalUserAsync(string provider, string providerKey, string? email, string? fullName)
    {
        // Try to find by external provider
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ExternalProvider == provider && u.ExternalId == providerKey);
        if (user != null)
        {
            return _mapper.Map<UserAuthResponse>(user);
        }

        // Try to find by email
        if (!string.IsNullOrEmpty(email))
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.ExternalProvider = provider;
                user.ExternalId = providerKey;
                user.IsEmailVerified = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return _mapper.Map<UserAuthResponse>(user);
            }
        }

        // Create new user
        var candidateRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate");
        if (candidateRole == null) return null;

        var newUser = new User
        {
            Email = email ?? $"{providerKey}@{provider}.local",
            FullName = fullName ?? "",
            RoleId = candidateRole.Id,
            PasswordHash = "", // no password
            ExternalProvider = provider,
            ExternalId = providerKey,
            IsEmailVerified = true
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserAuthResponse>(newUser);
    }

    public async Task<List<string>> GetPermissionsForRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return new List<string>();
        }

        return await _context.RolePermissions
            .Where(rp => rp.Role != null && rp.Role.Name == roleName)
            .Select(rp => rp.Permission != null ? rp.Permission.Name : string.Empty)
            .Where(name => name != string.Empty)
            .ToListAsync();
    }

    public async Task<int> GetResendCooldownSecondsAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return 0;

        var last = await _context.EmailVerifications
            .Where(e => e.UserId == user.Id)
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync();

        if (last == null) return 0;

        var createdAt = last.ExpiresAt.AddMinutes(-15);
        var secondsSince = (DateTime.UtcNow - createdAt).TotalSeconds;
        var remaining = ResendCooldownSeconds - (int)secondsSince;
        return remaining > 0 ? remaining : 0;
    }

    public async Task<BoolResponse> LockAccountAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return BoolResponse.Failure(ErrorMessage.UserNotFound);
        }

        if (string.Equals(user.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return BoolResponse.Failure("Không thể khóa tài khoản quản trị viên.");
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

    private static string GenerateOtpCode() =>
        RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    private static string GetOtpAttemptKey(int userId) => $"otp:attempts:{userId}";

}
