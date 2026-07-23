using Services.DTOs.CvBank;

namespace Services.Interfaces;

public interface ICvBankService
{
    // Đọc text từ PDF -> AI trích xuất -> lưu vào Kho CV của recruiter.
    Task<(bool ok, string? error)> AddFromPdfAsync(int recruiterId, string fileName, string storedFileName, byte[] pdfBytes, int? folderId = null);

    // Danh sách CV của recruiter (đã áp dụng bộ lọc).
    Task<List<CvBankItemDto>> GetForRecruiterAsync(int recruiterId, CvBankFilter filter);

    // Xoá 1 CV, trả về tên file đã lưu trên đĩa để controller xoá file vật lý (null nếu không tìm thấy).
    Task<string?> DeleteAsync(int recruiterId, int id);

    // Lấy thông tin file để phục vụ xem/tải PDF (kiểm tra quyền sở hữu).
    Task<(string storedFileName, string fileName)?> GetFileAsync(int recruiterId, int id);

    // ===== Thư mục phân loại CV =====
    Task<List<CvFolderDto>> GetFoldersAsync(int recruiterId);
    Task<(bool ok, string? error)> CreateFolderAsync(int recruiterId, string name);
    Task DeleteFolderAsync(int recruiterId, int folderId);
    Task MoveToFolderAsync(int recruiterId, int cvId, int? folderId);
}
