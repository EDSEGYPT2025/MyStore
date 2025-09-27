using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Pages.Manager.Products
{
    // لاحظ أن هذه الصفحة مؤمنة بسياسة "ManagerOnly" التي أنشأناها
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Product> ProductList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null)
            {
                // إذا لم يكن المستخدم مرتبطًا بمتجر، لا تسمح له بالوصول
                return Forbid();
            }

            // --- الجزء الأهم: جلب المنتجات التي تنتمي لمتجر هذا المستخدم فقط ---
            ProductList = await _context.Products
                .Include(p => p.Company) // لجلب اسم الماركة
                .Where(p => p.StoreId == user.StoreId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Page();
        }
    }
}
