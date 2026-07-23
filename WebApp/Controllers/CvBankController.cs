using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.CvBank;
using Services.Interfaces;

namespace WebApp.Controllers;

[Authorize(Roles = "Recruiter")]
public class CvBankController : Controller
{
    private const string UploadFolder = "uploads/cvbank";
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    private readonly ICvBankService _cvBankService;
    private readonly IWebHostEnvironment _env;

    public CvBankController(ICvBankService cvBankService, IWebHostEnvironment env)
    {
        _cvBankService = cvBankService;
        _env = env;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index(CvBankFilter filter)
    {
        var items = await _cvBankService.GetForRecruiterAsync(CurrentUserId, filter);
        ViewBag.Filter = filter;
        ViewBag.Folders = await _cvBankService.GetFoldersAsync(CurrentUserId);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFolder(string name, int? folderId)
    {
        var (ok, error) = await _cvBankService.CreateFolderAsync(CurrentUserId, name);
        if (ok) TempData["Success"] = "Đã tạo thư mục.";
        else TempData["Error"] = error;
        return RedirectToAction(nameof(Index), new { FolderId = folderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFolder(int id)
    {
        await _cvBankService.DeleteFolderAsync(CurrentUserId, id);
        TempData["Success"] = "Đã xoá thư mục (CV bên trong chuyển về chưa phân loại).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveToFolder(int cvId, int? folderId, int? currentFolder)
    {
        await _cvBankService.MoveToFolderAsync(CurrentUserId, cvId, folderId);
        TempData["Success"] = "Đã chuyển CV.";
        return RedirectToAction(nameof(Index), new { FolderId = currentFolder });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(200 * 1024 * 1024)]
    public async Task<IActionResult> Upload(List<IFormFile> files, int? folderId)
    {
        if (files == null || files.Count == 0)
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một file PDF.";
            return RedirectToAction(nameof(Index), new { FolderId = folderId });
        }

        var uploadPath = Path.Combine(_env.WebRootPath, UploadFolder);
        Directory.CreateDirectory(uploadPath);

        int success = 0;
        var errors = new List<string>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                || file.ContentType != "application/pdf")
            {
                errors.Add($"Chỉ hỗ trợ file PDF: {file.FileName}");
                continue;
            }

            if (file.Length > MaxFileSize)
            {
                errors.Add($"File quá lớn (tối đa 10MB): {file.FileName}");
                continue;
            }

            var storedFileName = $"{Guid.NewGuid():N}.pdf";
            var physicalPath = Path.Combine(uploadPath, storedFileName);

            byte[] bytes;
            using (var msRead = new MemoryStream())
            {
                await file.CopyToAsync(msRead);
                bytes = msRead.ToArray();
            }

            var (ok, error) = await _cvBankService.AddFromPdfAsync(
                CurrentUserId, Path.GetFileName(file.FileName), storedFileName, bytes, folderId);

            if (ok)
            {
                await System.IO.File.WriteAllBytesAsync(physicalPath, bytes);
                success++;
            }
            else
            {
                errors.Add(error ?? $"Lỗi xử lý: {file.FileName}");
            }
        }

        if (success > 0) TempData["Success"] = $"Đã phân tích và thêm {success} CV vào kho.";
        if (errors.Count > 0) TempData["Error"] = string.Join(" | ", errors);

        return RedirectToAction(nameof(Index), new { FolderId = folderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var storedFileName = await _cvBankService.DeleteAsync(CurrentUserId, id);
        if (storedFileName != null)
        {
            var physicalPath = Path.Combine(_env.WebRootPath, UploadFolder, storedFileName);
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
            TempData["Success"] = "Đã xoá CV khỏi kho.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ViewPdf(int id)
    {
        var file = await _cvBankService.GetFileAsync(CurrentUserId, id);
        if (file == null)
        {
            return NotFound();
        }

        var physicalPath = Path.Combine(_env.WebRootPath, UploadFolder, file.Value.storedFileName);
        if (!System.IO.File.Exists(physicalPath))
        {
            return NotFound();
        }

        return PhysicalFile(physicalPath, "application/pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(CvBankFilter filter)
    {
        var items = await _cvBankService.GetForRecruiterAsync(CurrentUserId, filter);

        var sb = new StringBuilder();
        sb.AppendLine("Tên,Email,Vị trí,KN tổng (năm),Loại,Kỹ năng,Tóm tắt");
        foreach (var c in items)
        {
            var loai = c.IsFresher ? "Fresher" : $"{c.TotalYearsExperience:0.#}y";
            sb.Append(Csv(c.Name)).Append(',')
              .Append(Csv(c.Email)).Append(',')
              .Append(Csv(c.CurrentTitle)).Append(',')
              .Append(Csv(c.TotalYearsExperience.ToString("0.#", CultureInfo.InvariantCulture))).Append(',')
              .Append(Csv(loai)).Append(',')
              .Append(Csv(string.Join("; ", c.Skills))).Append(',')
              .Append(Csv(c.Summary)).AppendLine();
        }

        // BOM để Excel đọc đúng tiếng Việt
        var bytes = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();

        return File(bytes, "text/csv", $"kho-cv-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }

    private static string Csv(string? value)
    {
        var v = value ?? string.Empty;
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n') || v.Contains('\r'))
        {
            v = "\"" + v.Replace("\"", "\"\"") + "\"";
        }
        return v;
    }
}
