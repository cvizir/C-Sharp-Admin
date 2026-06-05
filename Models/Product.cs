using System.ComponentModel.DataAnnotations;

namespace ProductManager.Models
{
    public class Product
    {
        public int Id { get; set; }

        // 新增：產品商務編碼
        [Required]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "編碼長度必須剛好為 8 碼")]
        [Display(Name = "產品編碼")]
        public string ProductCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "產品名稱為必填")]
        [Display(Name = "產品名稱")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "價格為必填")]
        [Range(0, double.MaxValue, ErrorMessage = "價格不能為負數")]
        [Display(Name = "價格")]
        public decimal Price { get; set; }

        [Display(Name = "產品描述")]
        public string? Description { get; set; }
        
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}