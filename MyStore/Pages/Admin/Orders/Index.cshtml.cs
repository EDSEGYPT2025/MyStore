using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Order> Orders { get; set; }

        public async Task OnGetAsync()
        {
            Orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate) // عرض الطلبات الأحدث أولاً
                .ToListAsync();
        }
    }
}
