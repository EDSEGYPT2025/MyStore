using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace MyStore.Pages.Admin.Stores
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "اسم المتجر مطلوب")]
            [Display(Name = "اسم المتجر")]
            public string Name { get; set; }

            [Required(ErrorMessage = "اسم المالك مطلوب")]
            [Display(Name = "اسم مالك المتجر")]
            public string OwnerName { get; set; }

            [Required(ErrorMessage = "رقم هاتف المالك مطلوب")]
            [Display(Name = "رقم هاتف المالك (مع كود الدولة)")]
            public string OwnerPhone { get; set; }
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Generate a unique slug for the store's URL
            var slug = await GenerateUniqueSlugAsync(Input.Name);

            // 2. Create the new Store entity
            var newStore = new Store
            {
                Name = Input.Name,
                Slug = slug,
                OwnerName = Input.OwnerName,
                OwnerPhone = Input.OwnerPhone,
                IsActive = true, // A new store is active by default
                CreatedAt = DateTime.UtcNow
            };

            // 3. Add to database and save
            _context.Stores.Add(newStore);
            await _context.SaveChangesAsync();

            // 4. Set a success message with the new store's link to show on the Index page
            var storeUrl = $"{Request.Scheme}://{Request.Host}/{newStore.Slug}";
            TempData["SuccessMessage"] = $"تم إنشاء متجر '{newStore.Name}' بنجاح! رابط المتجر هو: {storeUrl}";

            return RedirectToPage("./Index");
        }

        private async Task<string> GenerateUniqueSlugAsync(string baseName)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "store";

            // Normalize the string to be URL-friendly
            var slug = Regex.Replace(baseName.ToLower(), @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
            slug = slug.Substring(0, slug.Length <= 45 ? slug.Length : 45); // Limit length

            // Check for uniqueness
            var originalSlug = slug;
            int counter = 2;
            while (await _context.Stores.AnyAsync(s => s.Slug == slug))
            {
                slug = $"{originalSlug}-{counter}";
                counter++;
            }

            return slug;
        }
    }
}
