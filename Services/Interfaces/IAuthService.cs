using Domain.Entities;
using Services.DTOs.Auth;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<BoolResponse> RegisterCandidateAsync(RegisterRequest request);

    Task<BoolResponse> VerifyEmailOtpAsync(string email, string code);

    Task<BoolResponse> ResendEmailOtpAsync(string email);

    Task<int> GetResendCooldownSecondsAsync(string email);

    Task<UserAuthResponse?> GetOrCreateExternalUserAsync(string provider, string providerKey, string? email, string? fullName);

    Task<List<string>> GetPermissionsForRoleAsync(string roleName);

    Task<BoolResponse> LockAccountAsync(int userId);

    Task<BoolResponse> UnlockAccountAsync(int userId);
}
