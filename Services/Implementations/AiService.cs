using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Domain.Constraints;
using Services.DTOs.Application;
using Services.DTOs.CvBank;
using Services.Interfaces;

namespace Services.Implementations;

// Gọi AI server nội bộ (LAN) giống dự án D:\ARS: POST /api/chat trả về NDJSON streaming.
// BaseAddress + timeout + Authorization (Bearer) được cấu hình ở WebApp/Program.cs (AddHttpClient).
public class AiService : IAiService
{
    private readonly HttpClient _http;
    private readonly AiSettings _settings;

    public AiService(HttpClient http, AiSettings settings)
    {
        _http = http;
        _settings = settings;
    }

    public async Task<CvMatchResult> MatchCvAsync(string jobTitle, string description, string requirements, string cvText, JdEvalSettings settings)
    {
        if (string.IsNullOrWhiteSpace(cvText))
        {
            return CvMatchResult.Failure(ErrorMessage.CvContentMissing);
        }

        try
        {
            var text = await SendChatAsync(BuildMatchPrompt(jobTitle, description, requirements, cvText, settings));
            return ParseMatch(text);
        }
        catch (AiException ex)
        {
            return CvMatchResult.Failure(ex.Message);
        }
        catch (Exception)
        {
            return CvMatchResult.Failure(ErrorMessage.AiEvaluationError);
        }
    }

    public async Task<CvExtractResult> ExtractCvInfoAsync(string cvText)
    {
        if (string.IsNullOrWhiteSpace(cvText))
        {
            return CvExtractResult.Failure(ErrorMessage.CvContentMissing);
        }

        try
        {
            var text = await SendChatAsync(BuildExtractPrompt(cvText));
            return ParseExtract(text);
        }
        catch (AiException ex)
        {
            return CvExtractResult.Failure(ex.Message);
        }
        catch (Exception)
        {
            return CvExtractResult.Failure(ErrorMessage.AiEvaluationError);
        }
    }

    // POST /api/chat: đọc stream NDJSON từng dòng, ghép tất cả "content".
    // Dừng khi gặp {"done":true}; ném AiException khi gặp {"error":...}.
    private async Task<string> SendChatAsync(string message)
    {
        var payload = new
        {
            model = _settings.Model,
            message,
            history = new List<object>(),
            think = false
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "api/chat")
        {
            Content = JsonContent.Create(payload)
        };

        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

        if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new AiException(
                $"Máy chủ AI từ chối xác thực ({(int)resp.StatusCode}). " +
                "Hãy kiểm tra 'Ai:ApiKey' (Bearer token) trong appsettings.json rồi khởi động lại.");
        }
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        var sb = new StringBuilder();
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            line = line.Trim();
            if (line.Length == 0) continue;

            if (line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                line = line[5..].Trim();
            if (line.Length == 0 || line == "[DONE]") continue;

            JsonDocument doc;
            try { doc = JsonDocument.Parse(line); }
            catch { continue; }

            using (doc)
            {
                var root = doc.RootElement;

                if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
                    throw new AiException(err.GetString() ?? "Máy chủ AI báo lỗi.");

                if (root.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String)
                    sb.Append(c.GetString());

                if (root.TryGetProperty("done", out var done)
                    && (done.ValueKind == JsonValueKind.True
                        || (done.ValueKind == JsonValueKind.String && done.GetString() == "true")))
                    break;
            }
        }

        return sb.ToString();
    }

    // ===== Prompt: so khớp CV với JD =====
    private static string BuildMatchPrompt(string title, string description, string requirements, string cvText, JdEvalSettings s)
    {
        if (cvText.Length > 5000) cvText = cvText[..5000];

        var priority = string.IsNullOrWhiteSpace(s.PriorityNote)
            ? "(không có ghi chú ưu tiên đặc biệt)"
            : s.PriorityNote.Trim();

        return $@"Bạn là chuyên gia tuyển dụng. Hãy so khớp CV của ứng viên với Mô tả công việc (JD) và CHẤM ĐIỂM theo TRỌNG SỐ mà nhà tuyển dụng đặt ra.
CHỈ trả về JSON đúng schema sau, không thêm bất kỳ chữ nào khác:
{{
  ""match_score"": <số nguyên 0-100, điểm phù hợp tổng thể tính theo trọng số bên dưới>,
  ""verdict"": ""<một trong: Rất phù hợp | Phù hợp | Cân nhắc | Chưa phù hợp>"",
  ""breakdown"": {{
    ""experience"": <0-100>,
    ""skills"": <0-100>,
    ""education"": <0-100>,
    ""achievement"": <0-100>
  }},
  ""matched_skills"": [""kỹ năng/kinh nghiệm trong CV KHỚP yêu cầu JD""],
  ""missing_skills"": [""yêu cầu quan trọng của JD mà CV còn THIẾU""],
  ""strengths"": [""2-4 điểm mạnh nổi bật so với JD""],
  ""concerns"": [""2-4 điểm yếu / rủi ro so với JD""],
  ""summary"": ""2-3 câu tiếng Việt giải thích điểm số"",
  ""recommendation"": ""<một trong: Mời phỏng vấn | Cân nhắc thêm | Loại>""
}}

# TRỌNG SỐ CHẤM ĐIỂM (tổng 100) — match_score ≈ trung bình có trọng số của breakdown
- Kinh nghiệm liên quan: {s.WeightExperience}%
- Kỹ năng khớp yêu cầu: {s.WeightSkills}%
- Học vấn / chứng chỉ: {s.WeightEducation}%
- Thành tựu / dự án: {s.WeightAchievement}%

# ƯU TIÊN ĐẶC BIỆT CỦA NHÀ TUYỂN DỤNG (cho điểm cao nếu đáp ứng)
{priority}

Thang verdict: 85-100 Rất phù hợp; 70-84 Phù hợp; 50-69 Cân nhắc; dưới 50 Chưa phù hợp.

# MÔ TẢ CÔNG VIỆC
Vị trí: {title}
Mô tả: {description}
Yêu cầu: {requirements}

# CV ỨNG VIÊN
{cvText}";
    }

    private static CvMatchResult ParseMatch(string content)
    {
        var json = ExtractJson(content);
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var score = (int)Math.Clamp(ReadDouble(root, "match_score"), 0, 100);
            var verdict = ReadString(root, "verdict");
            var summary = ReadString(root, "summary");
            var matched = ReadList(root, "matched_skills");
            var missing = ReadList(root, "missing_skills");
            var strengths = ReadList(root, "strengths");
            var concerns = ReadList(root, "concerns");
            var recommendation = ReadString(root, "recommendation");

            var feedback = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(summary)) feedback.AppendLine(summary);
            if (strengths.Count > 0)
            {
                feedback.AppendLine();
                feedback.AppendLine("Điểm mạnh:");
                foreach (var s in strengths) feedback.AppendLine($"- {s}");
            }
            if (concerns.Count > 0)
            {
                feedback.AppendLine();
                feedback.AppendLine("Điểm cần lưu ý:");
                foreach (var c in concerns) feedback.AppendLine($"- {c}");
            }
            if (!string.IsNullOrWhiteSpace(recommendation))
            {
                feedback.AppendLine();
                feedback.AppendLine($"Đề xuất: {recommendation}");
            }

            var text = feedback.ToString().Trim();
            return new CvMatchResult
            {
                IsSuccess = true,
                MatchScore = score,
                Verdict = NullIfEmpty(verdict),
                MatchedSkills = matched,
                MissingSkills = missing,
                Strengths = strengths,
                Concerns = concerns,
                Summary = string.IsNullOrWhiteSpace(summary) ? text : summary,
                Recommendation = NullIfEmpty(recommendation),
                Feedback = string.IsNullOrWhiteSpace(text) ? content.Trim() : text
            };
        }
        catch
        {
            return CvMatchResult.Success(0, content.Trim());
        }
    }

    // ===== Prompt: trích xuất thông tin CV (cho Kho CV / cột ứng viên) =====
    private static string BuildExtractPrompt(string cvText)
    {
        if (cvText.Length > 6000) cvText = cvText[..6000];

        return $@"Bạn là chuyên gia nhân sự (HR senior). Đọc nội dung CV dưới đây và trả về JSON ĐÚNG schema sau, không thêm bất kỳ chữ nào khác:
{{
  ""name"": ""Họ tên ứng viên"",
  ""email"": ""email hoặc null"",
  ""phone"": ""số điện thoại hoặc null"",
  ""total_years_experience"": <số năm kinh nghiệm làm việc tổng, kiểu số thực, 0 nếu fresher>,
  ""ai_years_experience"": <số năm kinh nghiệm liên quan AI/ML/Data Science, 0 nếu không có>,
  ""is_fresher"": <true nếu chưa có kinh nghiệm đi làm chính thức (chỉ học/intern ngắn), ngược lại false>,
  ""current_title"": ""Chức danh/vị trí gần nhất hoặc null"",
  ""skills"": [""liệt kê tối đa 15 kỹ năng chính""],
  ""summary"": ""Tóm tắt 2-3 câu tiếng Việt về ứng viên"",
  ""strengths"": [""2-4 điểm mạnh nổi bật""],
  ""weaknesses"": [""2-4 điểm yếu / thiếu sót""]
}}

# NỘI DUNG CV
{cvText}";
    }

    private static CvExtractResult ParseExtract(string content)
    {
        var json = ExtractJson(content);
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var isFresher = false;
            if (root.TryGetProperty("is_fresher", out var f))
            {
                isFresher = f.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.String => bool.TryParse(f.GetString(), out var b) && b,
                    _ => false
                };
            }

            return new CvExtractResult
            {
                IsSuccess = true,
                Name = NullIfEmpty(ReadString(root, "name")),
                Email = NullIfEmpty(ReadString(root, "email")),
                Phone = NullIfEmpty(ReadString(root, "phone")),
                CurrentTitle = NullIfEmpty(ReadString(root, "current_title")),
                TotalYearsExperience = Math.Max(0, ReadDouble(root, "total_years_experience")),
                AiYearsExperience = Math.Max(0, ReadDouble(root, "ai_years_experience")),
                IsFresher = isFresher,
                Skills = ReadList(root, "skills"),
                Summary = NullIfEmpty(ReadString(root, "summary")),
                Strengths = ReadList(root, "strengths"),
                Weaknesses = ReadList(root, "weaknesses")
            };
        }
        catch
        {
            return CvExtractResult.Failure(ErrorMessage.AiEvaluationError);
        }
    }

    // ===== Helpers =====
    private static string? NullIfEmpty(string s) =>
        string.IsNullOrWhiteSpace(s) || s.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : s.Trim();

    private static string ExtractJson(string s)
    {
        int i = s.IndexOf('{');
        int j = s.LastIndexOf('}');
        return (i >= 0 && j > i) ? s.Substring(i, j - i + 1) : s;
    }

    private static string ReadString(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p)) return string.Empty;
        return p.ValueKind switch
        {
            JsonValueKind.String => p.GetString() ?? string.Empty,
            JsonValueKind.Number => p.ToString(),
            _ => string.Empty
        };
    }

    private static double ReadDouble(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p)) return 0;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var d)) return d;
        if (p.ValueKind == JsonValueKind.String && double.TryParse(p.GetString(), out var ds)) return ds;
        return 0;
    }

    private static List<string> ReadList(JsonElement el, string name)
    {
        var result = new List<string>();
        if (!el.TryGetProperty(name, out var p)) return result;

        if (p.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in p.EnumerateArray())
            {
                var v = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                if (!string.IsNullOrWhiteSpace(v)) result.Add(v!.Trim());
            }
        }
        else if (p.ValueKind == JsonValueKind.String)
        {
            foreach (var part in (p.GetString() ?? string.Empty)
                     .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                result.Add(part);
        }
        return result;
    }
}

// Lỗi nghiệp vụ từ máy chủ AI (dòng {"error":...} trong stream).
public class AiException : Exception
{
    public AiException(string message) : base(message) { }
}
