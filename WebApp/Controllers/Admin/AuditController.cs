using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models.Admin;

namespace WebApp.Controllers.Admin;

[Route("admin/audit")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "CanManageUsers")]
public class AuditController : Controller
{
    private readonly ARSDbContext _context;

    public AuditController(ARSDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? actionFilter, string? actorEmail, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 20)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(actionFilter))
            query = query.Where(a => a.Action.Contains(actionFilter));

        if (!string.IsNullOrWhiteSpace(actorEmail))
            query = query.Where(a => a.ActorEmail != null && a.ActorEmail.Contains(actorEmail));

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        var total = await query.CountAsync();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 5, 200);

        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogItem
            {
                Id = a.Id,
                ActorId = a.ActorId,
                ActorEmail = a.ActorEmail,
                Action = a.Action,
                TargetType = a.TargetType,
                TargetId = a.TargetId,
                Description = a.Description,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        var vm = new AuditListViewModel
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            ActionFilter = actionFilter,
            ActorEmailFilter = actorEmail,
            StartDate = startDate,
            EndDate = endDate
        };

        return View(vm);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(string? actionFilter, string? actorEmail, DateTime? startDate, DateTime? endDate, int[]? ids)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (ids != null && ids.Length > 0)
            query = query.Where(a => ids.Contains(a.Id));
        else
        {
            if (!string.IsNullOrWhiteSpace(actionFilter))
                query = query.Where(a => a.Action.Contains(actionFilter));
            if (!string.IsNullOrWhiteSpace(actorEmail))
                query = query.Where(a => a.ActorEmail != null && a.ActorEmail.Contains(actorEmail));
            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);
        }

        var list = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,CreatedAt,ActorId,ActorEmail,Action,TargetType,TargetId,Description");
        foreach (var a in list)
        {
            var desc = a.Description?.Replace("\n", " ").Replace("\r", " ") ?? string.Empty;
            sb.AppendLine($"{a.Id},{a.CreatedAt:O},{a.ActorId},{Quote(a.ActorEmail)},{Quote(a.Action)},{Quote(a.TargetType)},{a.TargetId},{Quote(desc)}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "audit_logs_export.csv");
    }

    private static string Quote(string? s)
    {
        if (s == null) return "";
        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }
}
