using Domain.Enums;
using Infrastructure;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Implementations;
using Services.Interfaces;
using WebApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Authentication configuration: Cookie + optional Google
var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "RecruitmentAuthCookie";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/404";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    // cookie used to temporarily store external authentication info
    .AddCookie("External", options =>
    {
        options.Cookie.Name = "ExternalAuthCookie";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = false;
    });

// Only add Google if client id/secret are configured
var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/external-login-callback";
        options.SignInScheme = "External";
        options.SaveTokens = true;
            // Handle remote failures (e.g. invalid client secret) gracefully by redirecting to login
            options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
            {
                OnRemoteFailure = context =>
                {
                    // Log details to help diagnose token endpoint errors
                    try
                    {
                        var loggerFactory = context.HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
                        var logger = loggerFactory?.CreateLogger("ExternalAuth");
                        logger?.LogError(context.Failure, "External authentication failed. Query: {Query}", context.Request.QueryString.Value);

                        context.HandleResponse();
                        // Redirect to login with a generic reason. Detailed cause is in server logs.
                        context.Response.Redirect("/login?reason=external_failed");
                    }
                    catch (System.Exception ex)
                    {
                        // Log unexpected errors during failure handling and rethrow in development
                        var loggerFactory = context.HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
                        var logger = loggerFactory?.CreateLogger("ExternalAuth");
                        logger?.LogError(ex, "Error handling remote failure");
                    }

                    return Task.CompletedTask;
                }
            };
    });
}
else
{
    // Log warning at startup if you want; skipping Google authentication because config is missing
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

builder.Services.AddDbContext<ARSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// Register services from the Services project
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<Services.Interfaces.IEmailService, EmailService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IJobPostingService, JobPostingService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<ICvBankService, CvBankService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();

// AI service noi bo (LAN, kieu Ollama/OpenWebUI) - giong du an D:\ARS.
builder.Services.AddSingleton(new AiSettings
{
    Model = builder.Configuration["Ai:Model"] ?? "gemma4:12b"
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


var app = builder.Build();

// Log if Google authentication is not configured
if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
{
    app.Logger.LogWarning("Google authentication is not configured. External login with Google will be disabled.");
}

// Tự động cập nhật hạn nộp hồ sơ của tất cả công việc thành năm 2027 khi ứng dụng chạy
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ARSDbContext>();
        dbContext.Database.ExecuteSqlRaw("UPDATE JobPostings SET ExpiredAt = '2027-12-31 23:59:59' WHERE ExpiredAt < '2027-01-01'");
    }
    catch
    {
        // Bỏ qua nếu DB chưa được khởi tạo
    }
}

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

app.UseAuthentication();
app.UseMiddleware<UserStatusValidationMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
