using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers.Admin;

// [BẢO VỆ] NGÀNH NGHỀ: CRUD JobCategory; chặn xóa ngành đang có JobPosting sử dụng.
[Authorize(Roles = "Admin")]
[Route("admin/categories")]
public class SkillController : Controller
{
    private readonly ARSDbContext _context;
    public SkillController(ARSDbContext context) => _context = context;
    [HttpGet] public async Task<IActionResult> Index() => View(await _context.JobCategories.Include(x => x.JobPostings).OrderBy(x => x.Name).ToListAsync());
    [HttpPost("save")][ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int? id, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Tên ngành nghề không được để trống."; return RedirectToAction(nameof(Index)); }
        var duplicate = await _context.JobCategories.AnyAsync(x => x.Name == name.Trim() && x.Id != id);
        if (duplicate) { TempData["Error"] = "Ngành nghề này đã tồn tại."; return RedirectToAction(nameof(Index)); }
        var item = id.HasValue ? await _context.JobCategories.FindAsync(id.Value) : new JobCategory();
        if (item is null) return NotFound();
        item.Name = name.Trim(); item.Description = description?.Trim();
        if (!id.HasValue) _context.JobCategories.Add(item);
        await _context.SaveChangesAsync(); TempData["Success"] = "Đã lưu ngành nghề."; return RedirectToAction(nameof(Index));
    }
    [HttpPost("delete")][ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.JobCategories.Include(x => x.JobPostings).FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        if (item.JobPostings.Count > 0) TempData["Error"] = "Không thể xóa ngành nghề đang có tin tuyển dụng.";
        else { _context.JobCategories.Remove(item); await _context.SaveChangesAsync(); TempData["Success"] = "Đã xóa ngành nghề."; }
        return RedirectToAction(nameof(Index));
    }
}
