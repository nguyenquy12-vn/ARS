namespace Domain.Enums;

public enum ApplicationStatus
{
    Pending = 1,    // Mới nộp, chờ xem
    Reviewing = 2,  // Nhà tuyển dụng đang xem hồ sơ
    EvaluatingAI = 3, // AI đang phân tích chấm điểm (nếu cần trạng thái chờ)
    Accepted = 4,   // Nhận vào phỏng vấn / Đạt
    Rejected = 5,   // Từ chối / Loại
    Withdrawn = 6   // Ứng viên tự rút lui / từ chối các bước sau
}