using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyStore.Pages.Manager.Orders
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

        public Order Order { get; set; }
        public List<CartItemViewModel> OrderItems { get; set; }

        // ViewModel to represent items in the cart
        public class CartItemViewModel
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
        }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null) return Forbid();

            // --- الأمان: جلب الطلب فقط إذا كان يخص متجر المستخدم الحالي ---
            Order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.StoreId == user.StoreId);

            if (Order == null)
            {
                return NotFound();
            }

            // --- تحليل تفاصيل الطلب من نص JSON ---
            if (!string.IsNullOrEmpty(Order.OrderDetailsJson))
            {
                try
                {
                    // Deserialize the JSON string into a list of CartItemViewModel
                    OrderItems = JsonSerializer.Deserialize<List<CartItemViewModel>>(Order.OrderDetailsJson);
                }
                catch (JsonException)
                {
                    // Handle cases where JSON might be malformed
                    OrderItems = new List<CartItemViewModel>();
                }
            }
            else
            {
                OrderItems = new List<CartItemViewModel>();
            }


            return Page();
        }
    }
}
