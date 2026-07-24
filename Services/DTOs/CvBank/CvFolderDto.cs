namespace Services.DTOs.CvBank;

public class CvFolderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }

    // JD của thư mục + cài đặt chấm điểm
    public string? JdDescription { get; set; }
    public string? JdRequirements { get; set; }
    public int AiWeightExperience { get; set; } = 35;
    public int AiWeightSkills { get; set; } = 40;
    public int AiWeightEducation { get; set; } = 10;
    public int AiWeightAchievement { get; set; } = 15;
    public string? AiPriorityNote { get; set; }
    public bool HasJd { get; set; }
}
