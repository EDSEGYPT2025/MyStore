using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;

namespace MyStore.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();

        public async Task OnGetAsync()
        {
            // نستخدم ViewModel لعرض بيانات مرتبطة من جداول مختلفة (Store و Company)
            Products = await _context.Products
                .Include(p => p.Store)      // جلب بيانات المتجر المرتبط
                .Include(p => p.Company)    // جلب بيانات الماركة المرتبطة
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    StoreName = p.Store.Name,      // اسم المتجر
                    CompanyName = p.Company.Name   // اسم الماركة
                })
                .OrderBy(p => p.StoreName)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        // ViewModel لعرض بيانات المنتج بشكل مخصص في الواجهة
        public class ProductViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string? ImageUrl { get; set; }
            public string StoreName { get; set; }
            public string CompanyName { get; set; }
        }
    }
}
