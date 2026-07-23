using Services.DTOs.Application;
using Services.DTOs.CvBank;

namespace Services.Interfaces;

public interface IAiService
{
    // So khớp CV của ứng viên với mô tả công việc (JD) bằng Gemini và trả về điểm + nhận xét.
    Task<CvMatchResult> MatchCvAsync(string jobTitle, string description, string requirements, string cvText);

    // Trích xuất thông tin có cấu trúc (tên, kinh nghiệm, skills...) từ nội dung text của một CV.
    Task<CvExtractResult> ExtractCvInfoAsync(string cvText);
}
