using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.Threading.Tasks;

namespace MyStore.Pages.Manager.Products
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Product Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null) return Forbid();

            // --- الأمان: جلب المنتج مع تفاصيل الماركة، فقط إذا كان المنتج يخص متجر المستخدم ---
            Product = await _context.Products
                .Include(p => p.Company)
                .FirstOrDefaultAsync(m => m.Id == id && m.StoreId == user.StoreId);

            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
