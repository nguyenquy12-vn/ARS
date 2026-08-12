using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers;

[Authorize(Roles = "Recruiter")]
public class VnPayController : Controller
{
    private const string SandboxPaymentUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    private readonly ARSDbContext _context;
    private readonly IConfiguration _configuration;

    public VnPayController(ARSDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePayment(int orderId)
    {
        var order = await _context.PaymentOrders.FirstOrDefaultAsync(x => x.Id == orderId && x.RecruiterId == CurrentUserId);
        if (order is null) return NotFound();
        if (order.Status != Domain.Enums.PaymentStatus.PendingConfirmation)
        {
            TempData["Error"] = "Đơn hàng này không còn chờ thanh toán.";
            return RedirectToAction("Index", "Billing");
        }

        var tmnCode = _configuration["VnPay:TmnCode"];
        var hashSecret = _configuration["VnPay:HashSecret"];
        if (string.IsNullOrWhiteSpace(tmnCode) || string.IsNullOrWhiteSpace(hashSecret))
        {
            TempData["VnPayError"] = "Cổng VNPAY chưa được cấu hình. Vui lòng liên hệ quản trị viên.";
            return RedirectToAction("Checkout", "Billing", new { id = order.Id });
        }

        var returnUrl = _configuration["VnPay:ReturnUrl"];
        if (string.IsNullOrWhiteSpace(returnUrl))
            returnUrl = $"{Request.Scheme}://{Request.Host}/VnPay/Return";

        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        var vnpIpAddress = remoteIp is null || IPAddress.IsLoopback(remoteIp)
            ? "127.0.0.1"
            : remoteIp.IsIPv4MappedToIPv6 ? remoteIp.MapToIPv4().ToString() : remoteIp.ToString();

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Amount"] = ((long)(order.Amount * 100)).ToString(CultureInfo.InvariantCulture),
            ["vnp_Command"] = "pay",
            ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = vnpIpAddress,
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Thanh toan don ARS {order.TransferCode}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TmnCode"] = tmnCode,
            ["vnp_TxnRef"] = order.Id.ToString(),
            ["vnp_Version"] = "2.1.0"
        };

        var query = BuildQuery(parameters);
        var signature = HmacSha512(hashSecret, query);
        order.AdminNote = "Đã khởi tạo thanh toán thẻ qua VNPay Sandbox; chờ kết quả trả về từ cổng thanh toán.";
        await _context.SaveChangesAsync();
        return Redirect($"{_configuration["VnPay:PaymentUrl"] ?? SandboxPaymentUrl}?{query}&vnp_SecureHash={signature}");
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Return()
    {
        var secureHash = Request.Query["vnp_SecureHash"].ToString();
        var txRef = Request.Query["vnp_TxnRef"].ToString();
        var hashSecret = _configuration["VnPay:HashSecret"];

        if (!int.TryParse(txRef, out var orderId) || string.IsNullOrWhiteSpace(hashSecret))
            return View("Result", (false, "Không thể xác thực kết quả VNPay."));

        var parameters = Request.Query
            .Where(x => x.Key.StartsWith("vnp_", StringComparison.Ordinal) && x.Key is not "vnp_SecureHash" and not "vnp_SecureHashType")
            .ToDictionary(x => x.Key, x => x.Value.ToString());
        var validSignature = string.Equals(HmacSha512(hashSecret, BuildQuery(parameters)), secureHash, StringComparison.OrdinalIgnoreCase);
        var order = await _context.PaymentOrders.FindAsync(orderId);
        if (order is null) return View("Result", (false, "Không tìm thấy đơn thanh toán."));

        var responseCode = Request.Query["vnp_ResponseCode"].ToString();
        var transactionNo = Request.Query["vnp_TransactionNo"].ToString();
        var amountValid = long.TryParse(Request.Query["vnp_Amount"], out var amount) && amount == (long)(order.Amount * 100);
        var isSuccessful = validSignature && amountValid && responseCode == "00";
        if (isSuccessful)
        {
            order.Status = Domain.Enums.PaymentStatus.Successful;
            order.ReviewedAt = DateTime.UtcNow;
        }
        order.AdminNote = isSuccessful
            ? $"VNPay Sandbox báo thanh toán thành công (mã GD: {transactionNo}). Gói đã được kích hoạt tự động."
            : "VNPay Sandbox trả về giao dịch không thành công hoặc chữ ký không hợp lệ.";
        await _context.SaveChangesAsync();

        return View("Result", (isSuccessful, order.AdminNote));
    }

    // VNPAY tạo checksum trên chuỗi query dùng WebUtility.UrlEncode (không dùng Uri.EscapeDataString).
    // Hai cách mã hóa khác nhau với khoảng trắng/ký tự đặc biệt sẽ gây lỗi "Sai chữ ký".
    private static string BuildQuery(IEnumerable<KeyValuePair<string, string>> values) =>
        string.Join("&", values.OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
    private static string HmacSha512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }
}
