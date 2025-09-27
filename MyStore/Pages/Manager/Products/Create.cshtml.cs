using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Pages.Manager.Products
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        public SelectList CompanyNameSL { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
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

            [Display(Name = "صورة المنتج")]
            public IFormFile UploadedImage { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null) return Forbid();

            // --- التعديل الجوهري: جلب الماركات الخاصة بالمتجر الحالي فقط ---
            var companies = await _context.Companies
                                          .Where(c => c.StoreId == user.StoreId)
                                          .OrderBy(c => c.Name)
                                          .ToListAsync();

            CompanyNameSL = new SelectList(companies, "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null) return Forbid();

            // --- حماية إضافية: التأكد من أن الماركة المختارة تابعة للمتجر ---
            var isCompanyOwnedByStore = await _context.Companies.AnyAsync(c => c.Id == Input.CompanyId && c.StoreId == user.StoreId);
            if (!isCompanyOwnedByStore)
            {
                ModelState.AddModelError("Input.CompanyId", "الماركة المختارة غير صالحة.");
            }

            if (!ModelState.IsValid)
            {
                // إذا كان هناك خطأ، أعد تحميل قائمة الماركات الخاصة بالمتجر فقط
                var companies = await _context.Companies.Where(c => c.StoreId == user.StoreId).ToListAsync();
                CompanyNameSL = new SelectList(companies, "Id", "Name");
                return Page();
            }

            var product = new Product
            {
                Name = Input.Name,
                Description = Input.Description,
                Price = Input.Price,
                CompanyId = Input.CompanyId,
                StoreId = user.StoreId.Value
            };

            if (Input.UploadedImage != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Input.UploadedImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.UploadedImage.CopyToAsync(fileStream);
                }
                product.ImageUrl = "/images/products/" + uniqueFileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}

