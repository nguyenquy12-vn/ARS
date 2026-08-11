using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;

namespace WebApp.Controllers;

[Authorize(Policy = "CanApplyJob")]
public class CandidateController : Controller
{
    private readonly IApplicationService _applicationService;

    public CandidateController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<IActionResult> MyApplications()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int candidateId))
        {
            TempData.Clear();
            return RedirectToAction("Login", "Auth");
        }

        var applications = await _applicationService.GetMyApplicationsAsync(candidateId);
        return View(applications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int applicationId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập lý do rút đơn.";
            return RedirectToAction(nameof(MyApplications));
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int candidateId))
        {
            TempData.Clear();
            return RedirectToAction("Login", "Auth");
        }

        var result = await _applicationService.WithdrawApplicationAsync(candidateId, applicationId, reason);
        TempData[result ? "SuccessMessage" : "ErrorMessage"] = result
            ? "Hủy đơn ứng tuyển thành công."
            : "Không thể hủy đơn ứng tuyển này.";

        return RedirectToAction(nameof(MyApplications));
    }
}
