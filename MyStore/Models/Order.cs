using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStore.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string? OrderNumber { get; set; } // رقم الطلب، يمكن إنشاؤه تلقائياً

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "رقم هاتف العميل مطلوب")]
        public string CustomerPhone { get; set; }

        public string? CustomerAddress { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // تم تغيير الاسم ليكون أوضح
        public string OrderDetailsJson { get; set; } // لتخزين تفاصيل المنتجات كـ JSON

        public DateTime OrderDate { get; set; } = DateTime.Now;

        // --- علاقة الربط الأساسية ---
        // هذا الحقل يضمن أن كل طلب يتبع لمتجر واحد فقط
        [Required]
        public int StoreId { get; set; }
        public virtual Store Store { get; set; }
    }
}

