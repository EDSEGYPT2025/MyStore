using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyStore.Data;
using MyStore.Models;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace MyStore.Pages.Manager.Companies
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

        [BindProperty]
        public InputModel Input { get; set; }

        // ViewModel لفصل بيانات الفورم عن نموذج قاعدة البيانات
        public class InputModel
        {
            [Required(ErrorMessage = "اسم الماركة مطلوب")]
            [Display(Name = "اسم الماركة")]
            public string Name { get; set; }

            [Display(Name = "شعار الماركة (اختياري)")]
            public IFormFile UploadedLogo { get; set; }
        }

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            // الآن نتحقق من صحة النموذج مباشرةً
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user?.StoreId == null)
            {
                // لا تسمح بالعملية إذا لم يكن المستخدم مرتبطًا بمتجر
                return Forbid();
            }

            // نقوم بإنشاء كائن جديد من نوع Company ونملأه بالبيانات
            var newCompany = new Company
            {
                Name = Input.Name,
                StoreId = user.StoreId.Value // الربط التلقائي بالمتجر
            };

            // --- منطق رفع الصورة (كما هو) ---
            if (Input.UploadedLogo != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "logos");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Input.UploadedLogo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.UploadedLogo.CopyToAsync(fileStream);
                }
                newCompany.LogoUrl = "/images/logos/" + uniqueFileName;
            }

            _context.Companies.Add(newCompany);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تمت إضافة الماركة '{newCompany.Name}' بنجاح.";
            return RedirectToPage("./Index");
        }
    }
}

