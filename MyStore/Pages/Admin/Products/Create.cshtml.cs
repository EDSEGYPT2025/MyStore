using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyStore.Data;
using MyStore.Models;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        // قوائم منسدلة للمتاجر والماركات
        public SelectList StoreSL { get; set; }
        public SelectList CompanySL { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "اسم المنتج مطلوب")]
            [Display(Name = "اسم المنتج")]
            public string Name { get; set; }

            [Display(Name = "وصف المنتج")]
            public string? Description { get; set; }

            [Required(ErrorMessage = "سعر المنتج مطلوب")]
            [Display(Name = "السعر")]
            [DataType(DataType.Currency)]
            [Range(0.01, 1000000.00, ErrorMessage = "الرجاء إدخال سعر صحيح")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "الرجاء اختيار المتجر (البائع)")]
            [Display(Name = "المتجر (البائع)")]
            public int StoreId { get; set; }

            [Required(ErrorMessage = "الرجاء اختيار الماركة (الشركة المصنّعة)")]
            [Display(Name = "الماركة (المصنّع)")]
            public int CompanyId { get; set; }

            [Display(Name = "صورة المنتج")]
            public IFormFile? UploadedImage { get; set; }
        }

        public void OnGet()
        {
            // جلب بيانات القوائم المنسدلة عند تحميل الصفحة
            PopulateDropdowns();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // في حالة وجود خطأ، يجب إعادة تعبئة القوائم المنسدلة
                PopulateDropdowns();
                return Page();
            }

            var newProduct = new Product
            {
                Name = Input.Name,
                Description = Input.Description,
                Price = Input.Price,
                StoreId = Input.StoreId,
                CompanyId = Input.CompanyId
            };

            // --- منطق رفع وحفظ الصورة (إن وجدت) ---
            if (Input.UploadedImage != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(Input.UploadedImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.UploadedImage.CopyToAsync(fileStream);
                }
                newProduct.ImageUrl = "/images/products/" + uniqueFileName;
            }

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم إضافة المنتج '{newProduct.Name}' بنجاح.";
            return RedirectToPage("./Index");
        }

        // دالة مساعدة لتجنب تكرار الكود
        private void PopulateDropdowns()
        {
            StoreSL = new SelectList(_context.Stores.OrderBy(s => s.Name), "Id", "Name");
            CompanySL = new SelectList(_context.Companies.OrderBy(c => c.Name), "Id", "Name");
        }
    }
}
