namespace Services.DTOs.CvBank;

// Bộ lọc cho danh sách Kho CV.
public class CvBankFilter
{
    // Tìm theo tên hoặc skill
    public string? Search { get; set; }

    // Kinh nghiệm tổng tối thiểu (năm)
    public double? MinTotalExperience { get; set; }

    // Kinh nghiệm AI tối thiểu (năm)
    public double? MinAiExperience { get; set; }

    // Loại: "" (tất cả) | "fresher" | "exp2" (KN tổng ≥ 2) | "ai3" (KN AI ≥ 3) | "any"
    public string? Type { get; set; }

    // Lọc theo thư mục: null = tất cả; giá trị dương = 1 thư mục; -1 = chưa phân loại
    public int? FolderId { get; set; }
}
