// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using ProductManager.Models;

namespace ProductManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // 代表資料庫中的 Products 資料表
        public DbSet<Product> Products { get; set; }

        // 在 AppDbContext 類別內加入：
        public DbSet<ProductImage> ProductImages { get; set; }

    }
}