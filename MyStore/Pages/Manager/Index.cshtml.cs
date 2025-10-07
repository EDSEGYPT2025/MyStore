using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Pages.Manager
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

        public Store CurrentStore { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }

        // English: A new property to hold the list of recent orders.
        // العربية: خاصية جديدة لعرض قائمة بأحدث الطلبات
        public IList<Order> RecentOrders { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.StoreId == null)
            {
                return Forbid();
            }

            CurrentStore = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == user.StoreId);

            if (CurrentStore == null)
            {
                return NotFound("لم يتم العثور على المتجر المرتبط بحسابك.");
            }

            ProductCount = await _context.Products.CountAsync(p => p.StoreId == CurrentStore.Id);
            OrderCount = await _context.Orders.CountAsync(o => o.StoreId == CurrentStore.Id);

            // English: Fetch the 5 most recent orders for the current store.
            // العربية: جلب أحدث 5 طلبات للمتجر الحالي
            RecentOrders = await _context.Orders
                .Where(o => o.StoreId == CurrentStore.Id)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }
}

