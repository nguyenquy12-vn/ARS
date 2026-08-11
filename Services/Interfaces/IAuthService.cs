using Domain.Entities;
using Services.DTOs.Auth;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<BoolResponse> RegisterCandidateAsync(RegisterRequest request);

    Task<bool> IsEmailAvailableAsync(string email);

    Task<LoginResponse> LoginWithGoogleAsync(string email, string fullName);

    Task<BoolResponse> ResetPasswordAsync(string email, string newPassword);
    Task<BoolResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

    Task<BoolResponse> LockAccountAsync(int userId);

    Task<BoolResponse> UnlockAccountAsync(int userId);
}
