using Services.DTOs.User;

namespace WebApp.Models.Admin;

public class UserListViewModel
{
    public IEnumerable<UserDto> Users { get; set; } = Enumerable.Empty<UserDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string? Search { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
