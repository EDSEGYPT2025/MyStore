using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Store
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المتجر مطلوب")]
        [Display(Name = "اسم المتجر")]
        public string Name { get; set; }

        [Required]
        public string Slug { get; set; }

        [Display(Name = "شعار المتجر")]
        public string? LogoUrl { get; set; } // رابط شعار المتجر (اختياري)

        [Required(ErrorMessage = "اسم المالك مطلوب")]
        [Display(Name = "اسم المالك")]
        public string OwnerName { get; set; }

        [Required(ErrorMessage = "رقم هاتف المالك مطلوب")]
        [Display(Name = "هاتف المالك")]
        public string OwnerPhone { get; set; }

        [Display(Name = "نشط؟")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- علاقات ---
        // كل متجر لديه قائمة من المنتجات
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        // كل متجر لديه قائمة من الطلبات
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        // كل متجر لديه قائمة من المستخدمين (أصحاب المتجر)
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}



