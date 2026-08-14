using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Company;
using Services.Interfaces;
using WebApp.Models.Company;
using WebApp.Filters;

namespace WebApp.Controllers;

// [BẢO VỆ] HỒ SƠ CÔNG TY: một Recruiter có một Company (RecruiterId unique).
// Edit POST gọi CompanyService; RecruiterId lấy từ claim để chống sửa công ty người khác.
[Authorize(Roles = "Recruiter")]
[RequireActiveRecruiterPlan]
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var profile = await _companyService.GetByRecruiterAsync(CurrentUserId);
        if (profile == null)
        {
            // Chưa có hồ sơ công ty -> chuyển sang form tạo mới
            return RedirectToAction(nameof(Edit));
        }

        return View(profile);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var profile = await _companyService.GetByRecruiterAsync(CurrentUserId);
        var model = profile?.Adapt<CompanyFormViewModel>() ?? new CompanyFormViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompanyFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = model.Adapt<CompanyFormRequest>();
        var result = await _companyService.SaveAsync(CurrentUserId, request);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Đã lưu hồ sơ công ty.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return View(model);
    }
}
