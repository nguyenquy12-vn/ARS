using Domain.Entities;
using Services.DTOs.Auth;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<BoolResponse> RegisterCandidateAsync(RegisterRequest request);

    Task<BoolResponse> LockAccountAsync(int userId);

    Task<BoolResponse> UnlockAccountAsync(int userId);
}
