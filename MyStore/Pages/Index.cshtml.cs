using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyStore.Pages
{
    // DTOs (Data Transfer Objects) for handling order data
    public class OrderDto
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public List<IncomingCartItemDto> CartItems { get; set; } = new();
    }

    public class IncomingCartItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }

    public class SavedCartItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Properties to hold data for the Razor page
        public List<Company> Companies { get; set; } = new();

        // === FIX: Added the missing FeaturedProducts property ===
        public List<Product> FeaturedProducts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Companies = await _context.Companies.ToListAsync();

            // === FIX: Populate the FeaturedProducts list ===
            // This will fetch the first 6 products to display on the main page.
            FeaturedProducts = await _context.Products
                .OrderBy(p => p.Id)
                .Take(6)
                .ToListAsync();
        }

        public async Task<JsonResult> OnGetProductsAsync(int companyId)
        {
            var products = await _context.Products
                .Where(p => p.CompanyId == companyId)
                .Select(p => new { p.Id, p.Name, p.Price, p.ImageUrl, p.Description })
                .ToListAsync();
            return new JsonResult(products);
        }

        [ValidateAntiForgeryToken]
        public async Task<JsonResult> OnPostCreateOrderAsync([FromBody] OrderDto orderData)
        {
            // Validation and order creation logic...
            if (orderData == null || orderData.CartItems == null || !orderData.CartItems.Any())
            {
                return new JsonResult(new { success = false, message = "سلة المشتريات فارغة." });
            }
            if (string.IsNullOrWhiteSpace(orderData.CustomerName) ||
                string.IsNullOrWhiteSpace(orderData.CustomerPhone) ||
                string.IsNullOrWhiteSpace(orderData.CustomerAddress))
            {
                return new JsonResult(new { success = false, message = "الرجاء التأكد من إدخال جميع بيانات التوصيل." });
            }
            try
            {
                var groupedCartItems = orderData.CartItems.GroupBy(item => item.Id).Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) }).ToList();
                var productIds = groupedCartItems.Select(item => item.ProductId).ToList();
                var productsFromDb = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
                decimal total = 0;
                var itemsToSave = new List<SavedCartItemDto>();
                foreach (var cartItem in groupedCartItems)
                {
                    var productFromDb = productsFromDb.FirstOrDefault(p => p.Id == cartItem.ProductId);
                    if (productFromDb != null)
                    {
                        if (cartItem.Quantity <= 0) continue;
                        itemsToSave.Add(new SavedCartItemDto { ProductName = productFromDb.Name, Quantity = cartItem.Quantity, UnitPrice = productFromDb.Price });
                        total += cartItem.Quantity * productFromDb.Price;
                    }
                }
                if (!itemsToSave.Any())
                {
                    return new JsonResult(new { success = false, message = "لم يتم العثور على منتجات صالحة في الطلب." });
                }
                var newOrder = new Order
                {
                    OrderNumber = $"ORD-{DateTime.UtcNow.Ticks}",
                    CustomerName = orderData.CustomerName,
                    CustomerPhone = orderData.CustomerPhone,
                    CustomerAddress = orderData.CustomerAddress,
                    TotalAmount = total,
                    OrderDetails = JsonSerializer.Serialize(itemsToSave),
                    OrderDate = DateTime.UtcNow
                };
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, orderNumber = newOrder.OrderNumber });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"خطأ داخلي: {ex.Message}" });
            }
        }
    }
}

