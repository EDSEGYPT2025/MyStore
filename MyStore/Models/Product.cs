using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStore.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "سعر المنتج مطلوب")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        // --- علاقات الربط ---

        // 1. الربط مع الشركة المصنّعة (ماركة المنتج)
        [Display(Name = "الشركة المصنّعة")]
        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }

        // 2. الربط مع المتجر (من يملك هذا المنتج) - هذا هو مفتاح عزل البيانات
        [Display(Name = "المتجر")]
        public int StoreId { get; set; }
        public virtual Store Store { get; set; }
    }
}
