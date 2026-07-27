using Services.DTOs.User;

namespace Services.Interfaces;

public interface IUserService
{
    // Get paged, filtered user list. Returns tuple (users, totalCount).
    Task<(List<UserDto> Users, int TotalCount)> GetUserListAsync(string? search, string? role, string? status, int page, int pageSize);

    Task<bool> IsUserLockedAsync(int userId);

    Task<UserDetailsResponse> GetUserByIdAsync(int userId);
}
