using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MyStore.Pages.Admin.Stores
{
    public class ManageUsersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageUsersModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Store Store { get; set; }
        public List<ApplicationUser> StoreUsers { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public int StoreId { get; set; }

            [Required(ErrorMessage = "الاسم الكامل مطلوب")]
            [Display(Name = "الاسم الكامل")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
            [EmailAddress]
            [Display(Name = "البريد الإلكتروني")]
            public string Email { get; set; }

            [Required(ErrorMessage = "كلمة المرور مطلوبة")]
            [StringLength(100, ErrorMessage = "يجب أن تكون كلمة المرور 6 أحرف على الأقل.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "كلمة المرور")]
            public string Password { get; set; }
        }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (Store == null) return NotFound();

            StoreUsers = await _context.Users.Where(u => u.StoreId == id).ToListAsync();

            Input = new InputModel { StoreId = Store.Id };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // إذا حدث خطأ، أعد تحميل البيانات اللازمة للصفحة
                await OnGetAsync(Input.StoreId);
                return Page();
            }

            // تحقق إذا كان البريد الإلكتروني مستخدمًا بالفعل
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Input.Email", "هذا البريد الإلكتروني مسجل بالفعل.");
                await OnGetAsync(Input.StoreId);
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,
                StoreId = Input.StoreId, // الربط الأهم: ربط المستخدم بالمتجر
                EmailConfirmed = true // تفعيل الحساب تلقائيًا
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // قم بتعيين دور "مدير المتجر" للمستخدم الجديد
                await _userManager.AddToRoleAsync(user, "StoreManager");
                return RedirectToPage(new { id = Input.StoreId });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // إذا فشلت عملية الإنشاء، أعد تحميل البيانات
            await OnGetAsync(Input.StoreId);
            return Page();
        }
    }
}
