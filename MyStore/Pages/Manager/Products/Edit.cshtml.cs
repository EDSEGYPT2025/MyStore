using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Pages.Manager.Products
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public SelectList CompanyNameSL { get; set; }
        public string CurrentImageUrl { get; set; }

        public class InputModel
        {
            public int Id { get; set; } // ضروري لمعرفة أي منتج يتم تعديله

            [Required(ErrorMessage = "اسم المنتج مطلوب")]
            [Display(Name = "اسم المنتج")]
            public string Name { get; set; }

            [Display(Name = "الوصف")]
            public string Description { get; set; }

            [Required(ErrorMessage = "سعر المنتج مطلوب")]
            [Display(Name = "السعر")]
            [Range(0.01, 100000.00, ErrorMessage = "الرجاء إدخال سعر صحيح")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "يجب اختيار ماركة للمنتج")]
            [Display(Name = "الماركة (الشركة المصنّعة)")]
            public int CompanyId { get; set; }

            [Display(Name = "تغيير صورة المنتج")]
            public IFormFile UploadedImage { get; set; } // الصورة اختيارية في التعديل
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null) return Forbid();

            // --- الأمان: جلب المنتج فقط إذا كان ينتمي لمتجر المستخدم الحالي ---
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.StoreId == user.StoreId);

            if (product == null)
            {
                // إذا كان المنتج غير موجود أو لا يخص هذا المتجر، لا تسمح بالوصول
                return NotFound();
            }

            Input = new InputModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CompanyId = product.CompanyId
            };

            CurrentImageUrl = product.ImageUrl;
            CompanyNameSL = new SelectList(_context.Companies, "Id", "Name", product.CompanyId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CompanyNameSL = new SelectList(_context.Companies, "Id", "Name", Input.CompanyId);
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null) return Forbid();

            // --- الأمان: مرة أخرى، تأكد من أن المنتج يخص المستخدم الحالي قبل التحديث ---
            var productToUpdate = await _context.Products.FirstOrDefaultAsync(p => p.Id == Input.Id && p.StoreId == user.StoreId);

            if (productToUpdate == null) return NotFound();

            // --- تحديث الصورة إذا تم رفع صورة جديدة ---
            if (Input.UploadedImage != null)
            {
                // حذف الصورة القديمة إذا كانت موجودة
                if (!string.IsNullOrEmpty(productToUpdate.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, productToUpdate.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // حفظ الصورة الجديدة
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Input.UploadedImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.UploadedImage.CopyToAsync(fileStream);
                }
                productToUpdate.ImageUrl = "/images/products/" + uniqueFileName;
            }

            // تحديث باقي بيانات المنتج
            productToUpdate.Name = Input.Name;
            productToUpdate.Description = Input.Description;
            productToUpdate.Price = Input.Price;
            productToUpdate.CompanyId = Input.CompanyId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث المنتج بنجاح!";
            return RedirectToPage("./Index");
        }
    }
}
