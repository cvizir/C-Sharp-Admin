// Models/Product.cs
using System.ComponentModel.DataAnnotations;

namespace ProductManager.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "產品名稱為必填")]
        [Display(Name = "產品名稱")]
        public string Name { get; set; }

        [Required(ErrorMessage = "價格為必填")]
        [Range(0, double.MaxValue, ErrorMessage = "價格不能為負數")]
        [Display(Name = "價格")]
        public decimal Price { get; set; }

        [Display(Name = "產品描述")]
        public string? Description { get; set; }
        
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 在 Product 類別內的最下方加入
        public List<ProductImage> Images { get; set; } = new List<ProductImage>();

    }
}