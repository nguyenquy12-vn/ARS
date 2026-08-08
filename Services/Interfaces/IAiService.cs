using Services.DTOs.Application;

namespace Services.Interfaces;

public interface IAiService
{
    // So khớp CV của ứng viên với mô tả công việc (JD) bằng Gemini và trả về điểm + nhận xét.
    Task<CvMatchResult> MatchCvAsync(string jobTitle, string description, string requirements, string cvText);
}
