using Microsoft.EntityFrameworkCore;
using ProductManager.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 註冊 AppDbContext 並使用 SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. 註冊 Cookie 驗證服務
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // 若未登入卻訪問受保護頁面，自動導向此路徑
        options.LoginPath = "/Account/Login";
        // 若權限不足的導向路徑
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // 設定 Token 過期時間
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ProductManager.Data.AppDbContext>();
    
    context.Database.Migrate(); // 自動套用遷移

    if (!context.Admins.Any())
    {
        // 初始 root 帳號
        context.Admins.Add(new ProductManager.Models.Admin
        {
            AdminCode = "ROOT0001",
            Account = "root",
            Name = "系統管理員",
            Password = BCrypt.Net.BCrypt.HashPassword("root"), // 密碼加密
            IsShow = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        context.SaveChanges();
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
app.UseRouting();

app.MapStaticAssets();


// 2. 啟用驗證與授權 (順序很重要，必須在 UseRouting 之後，UseAuthorization 之前)
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
