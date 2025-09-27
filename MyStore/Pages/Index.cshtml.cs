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
        public int StoreId { get; set; } // <<<< تم إضافة حقل معرّف المتجر
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

        // --- خصائص جديدة لعرض بيانات المتجر الحالي ---
        public Store CurrentStore { get; set; }
        public List<Company> Companies { get; set; } = new();
        public List<Product> Products { get; set; } = new();

        // --- الدالة الرئيسية لتحميل الصفحة ---
        public async Task<IActionResult> OnGetAsync(string storeSlug)
        {
            if (string.IsNullOrEmpty(storeSlug))
            {
                // في حالة عدم تحديد متجر، يتم عرض صفحة خطأ
                return NotFound("الرجاء تحديد رابط متجر صالح.");
            }

            // 1. البحث عن المتجر باستخدام الـ Slug من الرابط
            CurrentStore = await _context.Stores.FirstOrDefaultAsync(s => s.Slug == storeSlug);

            // 2. إذا لم يتم العثور على المتجر، يتم عرض خطأ 404
            if (CurrentStore == null)
            {
                return NotFound("هذا المتجر غير موجود.");
            }

            // 3. تحميل الماركات (Companies) التي لديها منتجات "في هذا المتجر فقط"
            Companies = await _context.Products
                                      .Where(p => p.StoreId == CurrentStore.Id)
                                      .Select(p => p.Company) // اختر الماركة المرتبطة بالمنتج
                                      .Distinct() // احصل على الماركات الفريدة فقط
                                      .OrderBy(c => c.Name)
                                      .ToListAsync();

            // 4. تحميل جميع منتجات هذا المتجر ليتم عرضها
            Products = await _context.Products
                                     .Where(p => p.StoreId == CurrentStore.Id)
                                     .Include(p => p.Company) // قم بتضمين بيانات الماركة مع المنتج
                                     .ToListAsync();

            return Page();
        }

        // --- دالة إنشاء الطلب المحدثة ---
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> OnPostCreateOrderAsync([FromBody] OrderDto orderData)
        {
            if (orderData == null || orderData.StoreId <= 0 || orderData.CartItems == null || !orderData.CartItems.Any())
            {
                return new JsonResult(new { success = false, message = "بيانات الطلب غير صالحة أو سلة المشتريات فارغة." });
            }

            var storeExists = await _context.Stores.AnyAsync(s => s.Id == orderData.StoreId);
            if (!storeExists)
            {
                return new JsonResult(new { success = false, message = "المتجر المحدد غير موجود." });
            }

            try
            {
                var productIds = orderData.CartItems.Select(item => item.Id).ToList();
                // تأمين إضافي: تأكد من أن المنتجات المطلوبة تتبع للمتجر الصحيح
                var productsFromDb = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && p.StoreId == orderData.StoreId)
                    .ToListAsync();

                decimal total = 0;
                var itemsToSave = new List<SavedCartItemDto>();

                foreach (var cartItem in orderData.CartItems)
                {
                    var productFromDb = productsFromDb.FirstOrDefault(p => p.Id == cartItem.Id);
                    if (productFromDb != null && cartItem.Quantity > 0)
                    {
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
                    OrderDetailsJson = JsonSerializer.Serialize(itemsToSave),
                    OrderDate = DateTime.UtcNow,
                    StoreId = orderData.StoreId // <<<< الأهم: ربط الطلب بالمتجر الصحيح
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, orderNumber = newOrder.OrderNumber });
            }
            catch (Exception)
            {
                // يفضل تسجيل الخطأ هنا
                return new JsonResult(new { success = false, message = "حدث خطأ غير متوقع أثناء معالجة الطلب." });
            }
        }
    }
}
