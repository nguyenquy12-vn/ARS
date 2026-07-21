using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // 1. Kiểm tra xem User đã Đăng nhập (Auth thành công) hay chưa
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Lấy các Claim cơ bản
                ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.UserName = User.FindFirstValue(ClaimTypes.Name);
                ViewBag.Email = User.FindFirstValue(ClaimTypes.Email);
                ViewBag.Role = User.FindFirstValue(ClaimTypes.Role);

                // Lấy TẤT CẢ các Claims có type là "Permission" nạp từ Cookie
                ViewBag.Permissions = User.Claims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToList();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
