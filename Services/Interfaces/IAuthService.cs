using Domain.Entities;
using Services.DTOs.Auth;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<RegisterResponse> RegisterCandidateAsync(RegisterRequest request);
}
