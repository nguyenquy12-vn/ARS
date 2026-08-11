namespace Services.Implementations;

// Cấu hình AI server nội bộ (LAN) — giống dự án D:\ARS.
public class AiSettings
{
    public string Model { get; set; } = "gemma4:12b";

    // ===== OpenAI (ChatGPT) — 1 API key dùng chung toàn hệ thống =====
    public string OpenAiBaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string? OpenAiApiKey { get; set; }
    public string OpenAiModel { get; set; } = "gpt-4o-mini";
}
