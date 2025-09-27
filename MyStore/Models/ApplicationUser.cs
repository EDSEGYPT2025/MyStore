using Microsoft.AspNetCore.Identity;
using MyStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// يمثل المستخدم في النظام، مع إضافة خصائص مخصصة مثل الاسم الكامل وربطه بالمتجر.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // خاصية مخصصة من الكود القديم
    [Required]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; }

    // --- الحقول الجديدة لربط المستخدم بالمتجر ---

    // هذا الحقل يربط المستخدم بمتجر واحد. 
    // إذا كان المستخدم "Admin" كبير، ستكون هذه القيمة فارغة (null).
    // إذا كان المستخدم "صاحب متجر"، ستحتوي على رقم المتجر الخاص به.
    public int? StoreId { get; set; }

    [ForeignKey("StoreId")]
    public virtual Store? Store { get; set; }
}

