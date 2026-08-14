using Domain.Enums;
using Infrastructure;
using LettuceEncrypt;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Implementations;
using Services.Interfaces;
using WebApp.Middlewares;
using WebApp.Accounts;
using WebApp.Realtime;

var builder = WebApplication.CreateBuilder(args);

// [BẢO VỆ] COMPOSITION ROOT: Program.cs là nơi ghép toàn hệ thống.
// Thứ tự cần nhớ: Configuration -> Authentication/Authorization -> DI Services
// -> DbContext -> Middleware pipeline -> MapControllerRoute.

// User Secrets mặc định chỉ được tự động nạp trong Development.
// Nạp tùy chọn để cấu hình local VNPAY/Google vẫn hoạt động khi chạy profile khác.
builder.Configuration.AddUserSecrets<Program>(optional: true);

// Cookie Authentication configuration
var authentication = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RecruitmentAuthCookie";
        options.LoginPath = "/login";           
        options.AccessDeniedPath = "/404";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); 
        options.SlidingExpiration = true;     
    });

// cấu hình đăng nhập google
var googleClientId = builder.Configuration["GoogleAuth:ClientId"];
var googleClientSecret = builder.Configuration["GoogleAuth:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    // đăng kí dịch vụ google
     authentication.AddCookie("External") 
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = "External";
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

builder.Services.AddAuthorization(options =>
{
    foreach (PermissionType permission in Enum.GetValues<PermissionType>())
    {
        string permissionName = permission.ToString();

        // Toàn bộ controller dùng quy ước "Can<Permission>" (vd: CanViewJob)
        options.AddPolicy($"Can{permissionName}", policy =>
            policy.RequireClaim("Permission", permissionName));
    }
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ARSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// Register services from the Services project
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IJobPostingService, JobPostingService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<ICvBankService, CvBankService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IRealtimeNotificationPublisher, SignalRNotificationPublisher>();
builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();

// AI service noi bo (LAN, kieu Ollama/OpenWebUI) - giong du an D:\ARS.
builder.Services.AddSingleton(new AiSettings
{
    Model = builder.Configuration["Ai:Model"] ?? "gemma4:12b",
    OpenAiBaseUrl = builder.Configuration["Ai:OpenAiBaseUrl"] ?? "https://api.openai.com/v1/",
    OpenAiApiKey = builder.Configuration["Ai:OpenAiApiKey"],
    OpenAiModel = builder.Configuration["Ai:OpenAiModel"] ?? "gpt-4o-mini"
});

// Biết user đang đăng nhập (để AiService chọn Local / ChatGPT theo cài đặt của họ)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, WebApp.Support.HttpCurrentUserContext>();

// HttpClient cho ChatGPT (OpenAI) - dùng key chung toàn hệ thống
builder.Services.AddHttpClient("openai", client =>
{
    var baseUrl = builder.Configuration["Ai:OpenAiBaseUrl"] ?? "https://api.openai.com/v1/";
    if (!baseUrl.EndsWith('/')) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);

    var timeout = builder.Configuration.GetValue<int?>("Ai:TimeoutSeconds") ?? 120;
    client.Timeout = TimeSpan.FromSeconds(timeout);

    var openAiKey = builder.Configuration["Ai:OpenAiApiKey"];
    if (!string.IsNullOrWhiteSpace(openAiKey))
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiKey);
});

builder.Services.AddHttpClient<IAiService, AiService>(client =>
{
    var baseUrl = builder.Configuration["Ai:BaseUrl"] ?? "http://localhost:11434/v1/";
    if (!baseUrl.EndsWith('/')) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);

    var timeout = builder.Configuration.GetValue<int?>("Ai:TimeoutSeconds") ?? 120;
    client.Timeout = TimeSpan.FromSeconds(timeout);

    var apiKey = builder.Configuration["Ai:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
});

// Register Mapster mappings
var config = TypeAdapterConfig.GlobalSettings;

config.Scan(typeof(MapsterConfig).Assembly);

builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, Mapper>();

// SSL tự động (Let's Encrypt) - chỉ bật khi có section "LettuceEncrypt" trong appsettings
if (builder.Configuration.GetSection("LettuceEncrypt").Exists())
{
    builder.Services.AddLettuceEncrypt()
        .PersistDataToDirectory(new DirectoryInfo("C:\\ARS\\ssl"), null);

    builder.WebHost.UseKestrel(k =>
    {
        k.ListenAnyIP(80);                       // cần cho ACME HTTP-01 challenge + redirect
        k.ListenAnyIP(443, o => o.UseHttps());   // LettuceEncrypt tự cấp cert cho 443
    });
}


var app = builder.Build();

// Tự động cập nhật hạn nộp hồ sơ của tất cả công việc thành năm 2027 khi ứng dụng chạy
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ARSDbContext>();
        dbContext.Database.Migrate();
        dbContext.Database.ExecuteSqlRaw("UPDATE JobPostings SET ExpiredAt = '2027-12-31 23:59:59' WHERE ExpiredAt < '2027-01-01'");
    }
    catch
    {
        // Bỏ qua nếu DB chưa được khởi tạo
    }
}

// Nhận header X-Forwarded-* từ IIS reverse proxy (để biết đúng scheme https)
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseMiddleware<UserStatusValidationMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
