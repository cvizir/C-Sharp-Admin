using System.ComponentModel.DataAnnotations;

namespace ProductManager.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        // 新增：圖片商務編碼
        [Required]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "編碼長度必須剛好為 8 碼")]
        [Display(Name = "圖片編碼")]
        public string ProductImageCode { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;
        
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}