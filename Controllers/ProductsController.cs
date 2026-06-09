using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManager.Data;
using ProductManager.Models;
using Microsoft.AspNetCore.Authorization;
namespace ProductManager.Controllers
{
    [Authorize] // 需要登入才能管理管理員
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment; // 用於取得 wwwroot 路徑

        // 注入 IWebHostEnvironment
        public ProductsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // R: 讀取列表 (包含圖片資料)
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Images).ToListAsync();
            return View(products);
        }

        public IActionResult Create() => View();

        // C: 新增產品與上傳圖片
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> imageFiles)
        {
            if (imageFiles != null && imageFiles.Count > 4)
            {
                ModelState.AddModelError("Images", "最多只能上傳 4 張圖片。");
            }

            // 自動產生唯一的產品編碼
            string productCode;
            do
            {
                productCode = Utils.CodeGenerator.Generate(8);
            } while (await _context.Products.AnyAsync(p => p.ProductCode == productCode));

            product.ProductCode = productCode;

            // 由於 ProductCode 是在後端動態產生的，需手動移除並重新驗證 Model 狀態
            ModelState.Remove(nameof(product.ProductCode));
            TryValidateModel(product);

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    await SaveProductImages(product.Id, imageFiles);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            // 載入產品時一併載入已有的圖片
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // U: 編輯產品與追加圖片
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> imageFiles, List<int>? deleteImageIds)
        {
            if (id != product.Id) return NotFound();

            // 取得資料庫中目前的圖片數量
            var currentImagesCount = await _context.ProductImages.CountAsync(img => img.ProductId == id);
            var deleteCount = deleteImageIds?.Count ?? 0;
            var newUploadCount = imageFiles?.Count ?? 0;

            // 驗證加總後的總數量是否超過 4 張
            if ((currentImagesCount - deleteCount + newUploadCount) > 4)
            {
                ModelState.AddModelError("Images", "產品圖片總數最多不能超過 4 張。");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // 處理刪除勾選的舊圖片
                    if (deleteImageIds != null && deleteImageIds.Count > 0)
                    {
                        var imagesToDelete = await _context.ProductImages.Where(img => deleteImageIds.Contains(img.Id)).ToListAsync();
                        foreach (var img in imagesToDelete)
                        {
                            // 刪除實體檔案
                            var filePath = Path.Combine(_environment.WebRootPath, img.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                            _context.ProductImages.Remove(img);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 處理新上傳的圖片
                    if (imageFiles != null && imageFiles.Count > 0)
                    {
                        await SaveProductImages(product.Id, imageFiles);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // 若驗證失敗，重新載入既有圖片供頁面渲染
            product.Images = await _context.ProductImages.Where(img => img.ProductId == id).ToListAsync();
            return View(product);
        }

        // 抽取出來的圖片驗證與儲存邏輯 (精簡程式碼)
        private async Task SaveProductImages(int productId, List<IFormFile> imageFiles)
        {
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");

            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            foreach (var file in imageFiles)
            {
                if (file.Length == 0 || file.Length > 2 * 1024 * 1024) continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext)) continue;

                var uniqueFileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 自動產生唯一的圖片編碼
                string imgCode;
                do
                {
                    imgCode = Utils.CodeGenerator.Generate(8);
                } while (await _context.ProductImages.AnyAsync(pi => pi.ProductImageCode == imgCode));

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImagePath = $"/uploads/products/{uniqueFileName}",
                    ProductImageCode = imgCode // 寫入新產生的編碼
                };
                _context.ProductImages.Add(productImage);
            }
            await _context.SaveChangesAsync();
        }
        private bool ProductExists(int id) => _context.Products.Any(e => e.Id == id);
    }
}