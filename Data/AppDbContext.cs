using Microsoft.EntityFrameworkCore;
using ProductManager.Models;

namespace ProductManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Admin> Admins { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 將 ProductCode 設為唯一索引
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ProductCode)
                .IsUnique();

            // 將 ProductImageCode 設為唯一索引
            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => pi.ProductImageCode)
                .IsUnique();
        }
    }
}