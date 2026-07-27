using System.Security.Claims;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models.Profile;

namespace WebApp.Controllers;

// Allow Admins to access recruiter profile editing as well
[Authorize(Roles = "Recruiter,Admin")]
public class ProfileController : Controller
{
    private readonly ARSDbContext _context;

    public ProfileController(ARSDbContext context)
    {
        _context = context;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user == null) return NotFound();

        var model = new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            SmtpHost = user.SmtpHost,
            SmtpPort = user.SmtpPort,
            SmtpUsername = user.SmtpUsername,
            SmtpPassword = user.SmtpPassword,
            SmtpFromEmail = user.SmtpFromEmail,
            SmtpEnableSsl = user.SmtpEnableSsl
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var current = await _context.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
            model.Email = current?.Email ?? string.Empty;
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user == null) return NotFound();

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber;
        user.SmtpHost = model.SmtpHost?.Trim();
        user.SmtpPort = model.SmtpPort;
        user.SmtpUsername = model.SmtpUsername?.Trim();
        user.SmtpPassword = model.SmtpPassword;
        user.SmtpFromEmail = model.SmtpFromEmail?.Trim();
        user.SmtpEnableSsl = model.SmtpEnableSsl;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã lưu hồ sơ cá nhân.";
        return RedirectToAction(nameof(Edit));
    }
}
