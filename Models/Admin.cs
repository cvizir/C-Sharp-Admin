// Models/Admin.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Models
{
    public class Admin
    {
        [Key] // 指定為 Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int No { get; set; } // 自動遞增流水號

        [Required]
        [StringLength(8)]
        public string AdminCode { get; set; } = string.Empty; // 8碼英數識別碼

        [Required(ErrorMessage = "帳號為必填")]
        [Display(Name = "登入帳號")]
        public string Account { get; set; } = string.Empty;

        [Required]
        [Display(Name = "密碼雜湊")]
        public string Password { get; set; } = string.Empty; // 儲存加密後的密碼

        [Display(Name = "顯示名稱")]
        public string? Name { get; set; } // 建議欄位：真實姓名或暱稱

        [Display(Name = "帳號狀態")]
        public int IsShow { get; set; } = 1; // 1: 啟用, 0: 停用

        [Display(Name = "最後登入時間")]
        public DateTime? LastLogin { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}