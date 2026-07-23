using Domain.Constraints;
using Domain.Entities;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.Application;
using Services.DTOs.CvBank;
using Services.Interfaces;

namespace Services.Implementations;

public class CvBankService : ICvBankService
{
    private readonly ARSDbContext _context;
    private readonly IAiService _aiService;

    public CvBankService(ARSDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<(bool ok, string? error)> AddFromPdfAsync(int recruiterId, string fileName, string storedFileName, byte[] pdfBytes, int? folderId = null)
    {
        // 1. Đọc PDF -> text
        string rawText;
        try
        {
            using var ms = new MemoryStream(pdfBytes);
            rawText = PdfTextExtractor.Extract(ms);
        }
        catch
        {
            return (false, $"Không đọc được nội dung PDF: {fileName}");
        }

        if (string.IsNullOrWhiteSpace(rawText))
        {
            return (false, $"PDF không có văn bản (có thể là bản scan ảnh): {fileName}");
        }

        // 2. Đưa text cho AI trích xuất thông tin
        var extracted = await _aiService.ExtractCvInfoAsync(rawText);
        if (!extracted.IsSuccess)
        {
            return (false, extracted.ErrorMessage ?? ErrorMessage.AiEvaluationError);
        }

        // 3. Lưu vào Kho CV
        var entry = new CvBankEntry
        {
            RecruiterId = recruiterId,
            FolderId = folderId,
            FileName = fileName,
            StoredFileName = storedFileName,
            Name = extracted.Name,
            Email = extracted.Email,
            Phone = extracted.Phone,
            CurrentTitle = extracted.CurrentTitle,
            TotalYearsExperience = extracted.TotalYearsExperience,
            AiYearsExperience = extracted.AiYearsExperience,
            IsFresher = extracted.IsFresher,
            Skills = extracted.Skills.Count > 0 ? string.Join(", ", extracted.Skills) : null,
            Summary = extracted.Summary,
            Strengths = extracted.Strengths.Count > 0 ? string.Join("\n", extracted.Strengths) : null,
            Weaknesses = extracted.Weaknesses.Count > 0 ? string.Join("\n", extracted.Weaknesses) : null,
            RawText = rawText,
            CreatedAt = DateTime.UtcNow
        };

        _context.CvBankEntries.Add(entry);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<CvBankItemDto>> GetForRecruiterAsync(int recruiterId, CvBankFilter filter)
    {
        var entries = await _context.CvBankEntries
            .Where(c => c.RecruiterId == recruiterId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CvBankItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                CurrentTitle = c.CurrentTitle,
                TotalYearsExperience = c.TotalYearsExperience,
                AiYearsExperience = c.AiYearsExperience,
                IsFresher = c.IsFresher,
                Skills = c.Skills != null
                    ? c.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string>(),
                Summary = c.Summary,
                Strengths = c.Strengths != null
                    ? c.Strengths.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string>(),
                Weaknesses = c.Weaknesses != null
                    ? c.Weaknesses.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string>(),
                FileName = c.FileName,
                StoredFileName = c.StoredFileName,
                CreatedAt = c.CreatedAt,
                FolderId = c.FolderId,
                MatchScore = c.MatchScore,
                MatchVerdict = c.MatchVerdict,
                MatchedSkills = c.MatchedSkills != null
                    ? c.MatchedSkills.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string>(),
                MissingSkills = c.MissingSkills != null
                    ? c.MissingSkills.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string>(),
                MatchStrengths = c.MatchStrengths != null
                    ? c.MatchStrengths.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string>(),
                MatchConcerns = c.MatchConcerns != null
                    ? c.MatchConcerns.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string>(),
                HasMatch = c.MatchScoredAt != null
            })
            .ToListAsync();

        // Lọc theo thư mục: -1 = chưa phân loại; >0 = 1 thư mục; null = tất cả
        if (filter.FolderId == -1)
        {
            entries = entries.Where(c => c.FolderId == null).ToList();
        }
        else if (filter.FolderId is > 0)
        {
            entries = entries.Where(c => c.FolderId == filter.FolderId).ToList();
        }

        // Lọc trong bộ nhớ (tập dữ liệu nhỏ theo từng recruiter)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var kw = filter.Search.Trim().ToLowerInvariant();
            entries = entries.Where(c =>
                (c.Name ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                (c.CurrentTitle ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                c.Skills.Any(s => s.ToLowerInvariant().Contains(kw)))
                .ToList();
        }

        if (filter.MinTotalExperience is > 0)
        {
            entries = entries.Where(c => c.TotalYearsExperience >= filter.MinTotalExperience!.Value).ToList();
        }

        if (filter.MinAiExperience is > 0)
        {
            entries = entries.Where(c => c.AiYearsExperience >= filter.MinAiExperience!.Value).ToList();
        }

        switch (filter.Type)
        {
            case "fresher":
                entries = entries.Where(c => c.IsFresher).ToList();
                break;
            case "exp2":
                entries = entries.Where(c => c.TotalYearsExperience >= 2).ToList();
                break;
            case "ai3":
                entries = entries.Where(c => c.AiYearsExperience >= 3).ToList();
                break;
            case "any":
                entries = entries.Where(c => c.IsFresher || c.TotalYearsExperience >= 2 || c.AiYearsExperience >= 3).ToList();
                break;
        }

        return entries;
    }

    public async Task<string?> DeleteAsync(int recruiterId, int id)
    {
        var entry = await _context.CvBankEntries
            .FirstOrDefaultAsync(c => c.Id == id && c.RecruiterId == recruiterId);
        if (entry == null)
        {
            return null;
        }

        var storedFileName = entry.StoredFileName;
        _context.CvBankEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return storedFileName;
    }

    public async Task<(string storedFileName, string fileName)?> GetFileAsync(int recruiterId, int id)
    {
        var entry = await _context.CvBankEntries
            .Where(c => c.Id == id && c.RecruiterId == recruiterId)
            .Select(c => new { c.StoredFileName, c.FileName })
            .FirstOrDefaultAsync();

        return entry == null ? null : (entry.StoredFileName, entry.FileName);
    }

    public async Task<List<CvFolderDto>> GetFoldersAsync(int recruiterId)
    {
        return await _context.CvFolders
            .Where(f => f.RecruiterId == recruiterId)
            .OrderBy(f => f.Name)
            .Select(f => new CvFolderDto
            {
                Id = f.Id,
                Name = f.Name,
                Count = f.CvBankEntries.Count
            })
            .ToListAsync();
    }

    public async Task<(bool ok, string? error)> CreateFolderAsync(int recruiterId, string name)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return (false, "Tên thư mục không được để trống.");
        }

        var exists = await _context.CvFolders
            .AnyAsync(f => f.RecruiterId == recruiterId && f.Name == name);
        if (exists)
        {
            return (false, "Đã có thư mục trùng tên.");
        }

        _context.CvFolders.Add(new CvFolder { RecruiterId = recruiterId, Name = name, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task DeleteFolderAsync(int recruiterId, int folderId)
    {
        var folder = await _context.CvFolders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.RecruiterId == recruiterId);
        if (folder == null) return;

        _context.CvFolders.Remove(folder); // CV bên trong tự về FolderId = null (SetNull)
        await _context.SaveChangesAsync();
    }

    public async Task MoveToFolderAsync(int recruiterId, int cvId, int? folderId)
    {
        var entry = await _context.CvBankEntries
            .FirstOrDefaultAsync(c => c.Id == cvId && c.RecruiterId == recruiterId);
        if (entry == null) return;

        // Nếu chỉ định thư mục, xác nhận thư mục thuộc recruiter này
        if (folderId is > 0)
        {
            var ok = await _context.CvFolders.AnyAsync(f => f.Id == folderId && f.RecruiterId == recruiterId);
            if (!ok) return;
        }

        entry.FolderId = folderId;
        await _context.SaveChangesAsync();
    }

    public async Task<CvFolderDto?> GetFolderAsync(int recruiterId, int folderId)
    {
        return await _context.CvFolders
            .Where(f => f.Id == folderId && f.RecruiterId == recruiterId)
            .Select(f => new CvFolderDto
            {
                Id = f.Id,
                Name = f.Name,
                Count = f.CvBankEntries.Count,
                JdDescription = f.JdDescription,
                JdRequirements = f.JdRequirements,
                AiWeightExperience = f.AiWeightExperience,
                AiWeightSkills = f.AiWeightSkills,
                AiWeightEducation = f.AiWeightEducation,
                AiWeightAchievement = f.AiWeightAchievement,
                AiPriorityNote = f.AiPriorityNote,
                HasJd = f.JdDescription != null || f.JdRequirements != null
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> SaveFolderJdAsync(int recruiterId, int folderId, string? description, string? requirements, JdEvalSettings settings)
    {
        var folder = await _context.CvFolders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.RecruiterId == recruiterId);
        if (folder == null) return false;

        folder.JdDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        folder.JdRequirements = string.IsNullOrWhiteSpace(requirements) ? null : requirements.Trim();
        folder.AiWeightExperience = Math.Clamp(settings.WeightExperience, 0, 100);
        folder.AiWeightSkills = Math.Clamp(settings.WeightSkills, 0, 100);
        folder.AiWeightEducation = Math.Clamp(settings.WeightEducation, 0, 100);
        folder.AiWeightAchievement = Math.Clamp(settings.WeightAchievement, 0, 100);
        folder.AiPriorityNote = string.IsNullOrWhiteSpace(settings.PriorityNote) ? null : settings.PriorityNote.Trim();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool ok, string? error, int score, string? verdict)> ScoreCvAsync(int recruiterId, int cvId)
    {
        var cv = await _context.CvBankEntries
            .Include(c => c.Folder)
            .FirstOrDefaultAsync(c => c.Id == cvId && c.RecruiterId == recruiterId);

        if (cv == null)
        {
            return (false, "Không tìm thấy CV.", 0, null);
        }
        if (cv.Folder == null)
        {
            return (false, "CV chưa thuộc thư mục nào để chấm theo JD.", 0, null);
        }
        var folder = cv.Folder;
        if (string.IsNullOrWhiteSpace(folder.JdDescription) && string.IsNullOrWhiteSpace(folder.JdRequirements))
        {
            return (false, "Thư mục chưa có JD. Vui lòng nhập JD trước khi chấm.", 0, null);
        }
        if (string.IsNullOrWhiteSpace(cv.RawText))
        {
            return (false, "CV không có nội dung text để chấm.", 0, null);
        }

        var settings = new JdEvalSettings
        {
            WeightExperience = folder.AiWeightExperience,
            WeightSkills = folder.AiWeightSkills,
            WeightEducation = folder.AiWeightEducation,
            WeightAchievement = folder.AiWeightAchievement,
            PriorityNote = folder.AiPriorityNote
        };

        var match = await _aiService.MatchCvAsync(
            folder.Name, folder.JdDescription ?? string.Empty, folder.JdRequirements ?? string.Empty, cv.RawText, settings);

        if (!match.IsSuccess)
        {
            return (false, match.ErrorMessage ?? "Lỗi chấm điểm AI.", 0, null);
        }

        cv.MatchScore = match.MatchScore;
        cv.MatchVerdict = match.Verdict;
        cv.MatchedSkills = match.MatchedSkills.Count > 0 ? string.Join("\n", match.MatchedSkills) : null;
        cv.MissingSkills = match.MissingSkills.Count > 0 ? string.Join("\n", match.MissingSkills) : null;
        cv.MatchStrengths = match.Strengths.Count > 0 ? string.Join("\n", match.Strengths) : null;
        cv.MatchConcerns = match.Concerns.Count > 0 ? string.Join("\n", match.Concerns) : null;
        cv.MatchScoredAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, null, match.MatchScore, match.Verdict);
    }
}
