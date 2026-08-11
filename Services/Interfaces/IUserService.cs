using Services.DTOs.User;

namespace Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetUserListAsync();

    Task<bool> IsUserLockedAsync(int userId);

    Task<UserDetailsResponse> GetUserByIdAsync(int userId);

    Task<(bool IsSuccess, string? ErrorMessage)> PromoteCandidateAsync(int userId, string targetRole);
}
