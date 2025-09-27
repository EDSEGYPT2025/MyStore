using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Pages.Manager.Orders
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Order> OrderList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null)
            {
                return Forbid();
            }

            // --- جلب الطلبات التي تنتمي لمتجر هذا المستخدم فقط ---
            OrderList = await _context.Orders
                .Where(o => o.StoreId == user.StoreId)
                .OrderByDescending(o => o.OrderDate) // عرض الطلبات الأحدث أولاً
                .ToListAsync();

            return Page();
        }
    }
}
