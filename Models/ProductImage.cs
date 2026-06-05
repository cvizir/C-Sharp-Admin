namespace ProductManager.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        
        // 儲存圖片在伺服器上的相對路徑（例如：/uploads/products/guid_filename.jpg）
        public string ImagePath { get; set; } = string.Empty;
        
        // 外鍵關聯
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}