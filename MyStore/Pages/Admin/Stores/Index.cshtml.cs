using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Pages.Admin.Stores
{
    public class IndexModel : PageModel
    {
        private readonly MyStore.Data.ApplicationDbContext _context;

        public IndexModel(MyStore.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        // --- هذا هو التعريف الناقص الذي يحل المشكلة ---
        public List<StoreViewModel> StoresList { get; set; }

        // هذا الكلاس الصغير يساعدنا على تجهيز البيانات بشكل منظم للعرض
        public class StoreViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Slug { get; set; }
            public string OwnerName { get; set; }
            public string OwnerPhone { get; set; }
            public string LogoUrl { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public string StoreUrl { get; set; }
        }

        public async Task OnGetAsync()
        {
            // نقوم بجلب بيانات المتاجر من قاعدة البيانات
            // ونحولها إلى قائمة الـ ViewModel التي تحتاجها الواجهة
            StoresList = await _context.Stores
                .OrderBy(s => s.Name)
                .Select(s => new StoreViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Slug = s.Slug,
                    OwnerName = s.OwnerName,
                    OwnerPhone = s.OwnerPhone,
                    LogoUrl = s.LogoUrl,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    // نقوم بإنشاء الرابط الكامل للمتجر هنا
                    StoreUrl = Url.Page("/Index", null, new { storeSlug = s.Slug }, Request.Scheme)
                })
                .ToListAsync();
        }

        // هذه الدالة مسؤولة عن تفعيل وإيقاف المتجر
        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store != null)
            {
                store.IsActive = !store.IsActive; // عكس الحالة الحالية
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}

