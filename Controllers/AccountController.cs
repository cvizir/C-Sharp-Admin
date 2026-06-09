// Controllers/AccountController.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManager.Data;
using System.Security.Claims;

namespace ProductManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // 顯示登入頁面
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 處理登入請求
        [HttpPost]
        [ValidateAntiForgeryToken] // 防止 CSRF 惡意攻擊的 Token 驗證
        public async Task<IActionResult> Login(string account, string password)
        {
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "請輸入帳號與密碼");
                return View();
            }

            // 1. 尋找帳號
            var admin = await _context.Admins.SingleOrDefaultAsync(a => a.Account == account);
            
            if (admin == null || !BCrypt.Net.BCrypt.Verify(password, admin.Password))
            {
                ModelState.AddModelError("", "帳號或密碼錯誤");
                return View();
            }

            // 2. 檢查 IsShow 權限 (0 則阻擋登入)
            if (admin.IsShow == 0)
            {
                ModelState.AddModelError("", "此帳號已被停用，請聯繫系統管理員");
                return View();
            }

            // 3. 登入成功，核發身分憑證 Token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.Account),
                new Claim("AdminCode", admin.AdminCode)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity));

            // 登入後導向產品列表
            // return RedirectToAction("Index", "Products");
            return RedirectToAction("Index", "Admins");
        }

        // 登出
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}