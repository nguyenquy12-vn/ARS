using Domain.Enums;
using Services.DTOs.Application;

namespace Services.Interfaces;

public interface IApplicationService
{
    Task<bool> ApplyJobAsync(int candidateId, int jobId, string cvFilePath, string cvFileName, string? coverLetter);
    Task<List<ApplicationDto>> GetMyApplicationsAsync(int candidateId);
    Task<bool> WithdrawApplicationAsync(int candidateId, int applicationId, string reason);

    // Danh sách ứng viên đã nộp vào một tin tuyển dụng (kiểm tra quyền sở hữu theo recruiter)
    Task<List<ApplicantListItem>> GetApplicantsForJobAsync(int jobId, int recruiterId);

    // Chi tiết một hồ sơ ứng tuyển
    Task<ApplicationDetail?> GetDetailAsync(int applicationId, int recruiterId);

    // Cập nhật trạng thái duyệt hồ sơ (Reviewing / Accepted / Rejected...)
    Task<ApplicationResult> UpdateStatusAsync(int applicationId, int recruiterId, ApplicationStatus status);

    // Chấm điểm CV bằng Gemini AI và lưu kết quả (AiMatchScore + AiFeedback)
    Task<ApplicationResult> EvaluateWithAiAsync(int applicationId, int recruiterId);
}
