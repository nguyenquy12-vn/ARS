using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;
using Services.Interfaces;
using Infrastructure;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IJobPostingService _jobPostingService;
        private readonly ARSDbContext _context;

        public HomeController(ILogger<HomeController> logger, IJobPostingService jobPostingService, ARSDbContext context)
        {
            _logger = logger;
            _jobPostingService = jobPostingService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Recruiter không dùng trang chủ candidate/guest — dashboard của họ chính là trang quản lý tin
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Recruiter"))
            {
                return RedirectToAction("Index", "JobPosting");
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.UserName = User.FindFirstValue(ClaimTypes.Name);
                ViewBag.Email = User.FindFirstValue(ClaimTypes.Email);
                ViewBag.Role = User.FindFirstValue(ClaimTypes.Role);
                ViewBag.Permissions = User.Claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
            }

            ViewBag.LatestJobs = await _jobPostingService.GetLatestJobsAsync(6);
            ViewBag.Categories = await _context.JobCategories.ToListAsync();

            return View();
        }

        public IActionResult Privacy() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
