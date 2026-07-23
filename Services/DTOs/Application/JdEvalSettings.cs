namespace Services.DTOs.Application;

// Cài đặt chấm điểm AI cho một tin tuyển dụng (trọng số các tiêu chí + ghi chú ưu tiên).
public class JdEvalSettings
{
    public int WeightExperience { get; set; } = 35;
    public int WeightSkills { get; set; } = 40;
    public int WeightEducation { get; set; } = 10;
    public int WeightAchievement { get; set; } = 15;
    public string? PriorityNote { get; set; }
}
