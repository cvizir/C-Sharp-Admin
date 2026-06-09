using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManager.Data;
using ProductManager.Models;

namespace ProductManager.Controllers
{
    [Authorize]
    public class AdminsController : Controller
    {
        private readonly AppDbContext _context;

        public AdminsController(AppDbContext context)
        {
            _context = context;
        }

        // R: 讀取列表
        public async Task<IActionResult> Index()
        {
            var admins = await _context.Admins.ToListAsync();
            return View(admins);
        }

        // C: 新增頁面 (GET)
        public IActionResult Create()
        {
            return View();
        }

        // C: 新增處理 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string account, string? name, string password, int isShow)
        {
            // 檢查帳號是否重複
            if (await _context.Admins.AnyAsync(a => a.Account == account))
            {
                ModelState.AddModelError("Account", "此帳號已被使用。");
            }

            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Password", "密碼為必填欄位。");
            }

            if (ModelState.IsValid)
            {
                // 產生 8 碼唯一 AdminCode
                string code;
                do { code = Utils.CodeGenerator.Generate(8); } 
                while (await _context.Admins.AnyAsync(a => a.AdminCode == code));

                var admin = new Admin
                {
                    AdminCode = code,
                    Account = account,
                    Name = name,
                    Password = BCrypt.Net.BCrypt.HashPassword(password), // 加密
                    IsShow = isShow,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // 若驗證失敗，將資料帶回頁面
            ViewBag.Account = account;
            ViewBag.Name = name;
            return View();
        }

        // U: 編輯頁面 (GET)
        [Route("Admins/Edit/{adminCode}")]
        public async Task<IActionResult> Edit(string adminCode)
        {
            if (string.IsNullOrEmpty(adminCode)) return NotFound();

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.AdminCode == adminCode);
            if (admin == null) return NotFound();

            return View(admin);
        }

        // U: 編輯處理 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admins/Edit/{adminCode}")]
        // 👉 改為直接接收表單特定欄位，避開整份 Model 驗證失敗無法寫入的 Bug
        public async Task<IActionResult> Edit(string adminCode, string? name, int isShow, string? newPassword)
        {
            var dbAdmin = await _context.Admins.FirstOrDefaultAsync(a => a.AdminCode == adminCode);
            if (dbAdmin == null) return NotFound();

            if (ModelState.IsValid)
            {
                dbAdmin.Name = name;
                dbAdmin.IsShow = isShow;
                dbAdmin.UpdatedAt = DateTime.Now;

                // 如果有輸入新密碼才進行變更
                if (!string.IsNullOrEmpty(newPassword))
                {
                    dbAdmin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                }

                _context.Update(dbAdmin);
                await _context.SaveChangesAsync(); // 確實寫入 SQLite
                
                return RedirectToAction(nameof(Index));
            }

            return View(dbAdmin);
        }

        // D: 刪除處理 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admins/Delete/{adminCode}")]
        public async Task<IActionResult> Delete(string adminCode)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.AdminCode == adminCode);
            
            // 安全限制：找到帳號，且該帳號不能是 root，也不能是目前登入的自己
            if (admin != null)
            {
                if (admin.Account == "root")
                {
                    TempData["ErrorMessage"] = "系統保護：不允許刪除 root 帳號。";
                    return RedirectToAction(nameof(Index));
                }

                if (admin.Account == User.Identity?.Name)
                {
                    TempData["ErrorMessage"] = "不允許刪除目前登入中的帳號。";
                    return RedirectToAction(nameof(Index));
                }

                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}