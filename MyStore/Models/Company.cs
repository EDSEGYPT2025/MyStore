using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStore.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الماركة مطلوب")]
        [Display(Name = "اسم الماركة")]
        public string Name { get; set; }

        [Display(Name = "شعار الماركة")]
        public string? LogoUrl { get; set; }

        // --- الربط بالمتجر (التعديل الجوهري) ---
        // كل ماركة الآن أصبحت تابعة لمتجر واحد فقط
        [Required]
        public int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }
        // -----------------------------------------

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

