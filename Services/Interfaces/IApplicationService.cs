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
    Task<List<ApplicantListItem>> GetUpcomingInterviewsAsync(int recruiterId);

    // Trích xuất thông tin CV bằng AI cho các ứng viên chưa phân tích. Trả về số CV đã phân tích.
    Task<int> AnalyzeApplicantsAsync(int jobId, int recruiterId);

    // Chấm điểm ứng viên theo JD. rescoreAll=false: chỉ chấm ứng viên chưa chấm; true: chấm lại tất cả.
    Task<(int scored, string? error)> ScoreApplicantsAsync(int jobId, int recruiterId, bool rescoreAll);

    // Chấm điểm 1 ứng viên (dùng cho tiến trình từng bước). Trả về điểm + kết luận.
    Task<(bool ok, string? error, int score, string? verdict)> ScoreApplicantAsync(int applicationId, int recruiterId);

    // Lưu cài đặt trọng số chấm điểm cho một tin tuyển dụng.
    Task<bool> SaveJdSettingsAsync(int jobId, int recruiterId, JdEvalSettings settings);

    // ===== Phía ứng viên (Candidate) =====
    // Ứng tuyển: upload CV (PDF) + lời nhắn -> tạo Resume + Application.
    Task<(bool ok, string? error)> ApplyAsync(int jobId, int candidateId, string fileName, string filePath, byte[] pdfBytes, string? coverLetter);

    // Kiểm tra ứng viên đã ứng tuyển tin này chưa.
    Task<bool> HasAppliedAsync(int jobId, int candidateId);

    // Chi tiết một hồ sơ ứng tuyển
    Task<ApplicationDetail?> GetDetailAsync(int applicationId, int recruiterId);

    // Cập nhật trạng thái duyệt hồ sơ (Reviewing / Accepted / Rejected...)
    Task<ApplicationResult> UpdateStatusAsync(int applicationId, int recruiterId, ApplicationStatus status);

    // Hẹn lịch phỏng vấn cho ứng viên + gửi email mời. Trả về (ok, lỗi, thông tin gửi mail).
    Task<(bool ok, string? error, string? mailInfo)> ScheduleInterviewAsync(int applicationId, int recruiterId, DateTime interviewAt, string? note);

    // Gửi mời phỏng vấn HÀNG LOẠT cho nhiều ứng viên cùng lúc. Trả về (số gửi ok, số lỗi, thông tin).
    Task<(int ok, int failed, string info)> BulkScheduleInterviewAsync(int jobId, int recruiterId, IEnumerable<int> applicationIds, DateTime interviewAt, string? note);

    // Chấm điểm CV bằng Gemini AI và lưu kết quả (AiMatchScore + AiFeedback)
    Task<ApplicationResult> EvaluateWithAiAsync(int applicationId, int recruiterId);
}
