using System.Text.Json;
using Domain.Constraints;
using Mscc.GenerativeAI;
using Services.DTOs.Application;
using Services.Interfaces;

namespace Services.Implementations;

// Chấm điểm độ phù hợp giữa CV ứng viên và mô tả công việc (JD) bằng Google Gemini.
public class GeminiAiService : IAiService
{
    private readonly string? _apiKey;
    private readonly string _model;

    public GeminiAiService(string? apiKey, string model)
    {
        _apiKey = apiKey;
        _model = string.IsNullOrWhiteSpace(model) ? "gemini-2.0-flash" : model;
    }

    public async Task<CvMatchResult> MatchCvAsync(string jobTitle, string description, string requirements, string cvText)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains("YourActual"))
        {
            return CvMatchResult.Failure(ErrorMessage.AiNotConfigured);
        }

        if (string.IsNullOrWhiteSpace(cvText))
        {
            return CvMatchResult.Failure(ErrorMessage.CvContentMissing);
        }

        try
        {
            var googleAi = new GoogleAI(apiKey: _apiKey);
            var model = googleAi.GenerativeModel(model: _model);

            var prompt = BuildPrompt(jobTitle, description, requirements, cvText);
            var response = await model.GenerateContent(prompt);

            var text = response?.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return CvMatchResult.Failure(ErrorMessage.AiEvaluationError);
            }

            return Parse(text);
        }
        catch (Exception)
        {
            return CvMatchResult.Failure(ErrorMessage.AiEvaluationError);
        }
    }

    private static string BuildPrompt(string title, string description, string requirements, string cvText)
    {
        // Giới hạn độ dài CV để tránh vượt token
        if (cvText.Length > 8000) cvText = cvText[..8000];

        return $@"Bạn là chuyên gia tuyển dụng. Hãy so khớp CV của ứng viên với Mô tả công việc (JD).
CHỈ trả về JSON đúng schema sau, không thêm bất kỳ chữ nào khác:
{{
  ""match_score"": <số nguyên 0-100, mức độ phù hợp tổng thể>,
  ""summary"": ""2-3 câu tiếng Việt giải thích vì sao đạt điểm như vậy"",
  ""strengths"": [""2-4 điểm mạnh so với JD""],
  ""concerns"": [""2-4 điểm còn thiếu hoặc rủi ro""],
  ""recommendation"": ""<một trong: Mời phỏng vấn | Cân nhắc thêm | Loại>""
}}
Thang điểm: 85-100 Rất phù hợp; 70-84 Phù hợp; 50-69 Cân nhắc; dưới 50 Chưa phù hợp.

# MÔ TẢ CÔNG VIỆC
Vị trí: {title}
Mô tả: {description}
Yêu cầu: {requirements}

# CV ỨNG VIÊN
{cvText}";
    }

    private static CvMatchResult Parse(string content)
    {
        var json = ExtractJson(content);
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var score = (int)Math.Clamp(ReadDouble(root, "match_score"), 0, 100);
            var recommendation = ReadString(root, "recommendation");
            var summary = ReadString(root, "summary");
            var strengths = ReadList(root, "strengths");
            var concerns = ReadList(root, "concerns");

            var feedback = new System.Text.StringBuilder();
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
            return CvMatchResult.Success(score, string.IsNullOrWhiteSpace(text) ? content.Trim() : text);
        }
        catch
        {
            // Model trả về text không phải JSON chuẩn -> vẫn hiển thị nội dung, điểm 0.
            return CvMatchResult.Success(0, content.Trim());
        }
    }

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
            {
                result.Add(part);
            }
        }
        return result;
    }
}
