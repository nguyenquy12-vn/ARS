namespace Domain.Enums;

// Nguồn AI mà tài khoản sử dụng để chấm/điểm & trích xuất CV.
public enum AiProvider
{
    Local = 0,   // Máy chủ AI nội bộ (mặc định)
    OpenAI = 1   // ChatGPT (OpenAI) - dùng API key chung của hệ thống
}
