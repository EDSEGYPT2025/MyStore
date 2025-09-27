using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.IO;
using System.Threading.Tasks;

namespace MyStore.Pages.Manager.Products
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        [BindProperty]
        public Product Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null) return Forbid();

            Product = await _context.Products
                .Include(p => p.Company)
                .FirstOrDefaultAsync(m => m.Id == id && m.StoreId == user.StoreId);

            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null) return Forbid();

            // --- الأمان: ابحث عن المنتج مرة أخرى للتأكد من أنه يخص المستخدم ---
            var productToDelete = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.StoreId == user.StoreId);

            if (productToDelete != null)
            {
                // --- حذف الصورة من السيرفر قبل حذف المنتج من قاعدة البيانات ---
                if (!string.IsNullOrEmpty(productToDelete.ImageUrl))
                {
                    var imagePath = Path.Combine(_hostEnvironment.WebRootPath, productToDelete.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(productToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف المنتج بنجاح.";
            }

            return RedirectToPage("./Index");
        }
    }
}
