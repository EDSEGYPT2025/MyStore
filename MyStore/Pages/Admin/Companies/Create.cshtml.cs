using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyStore.Data;
using MyStore.Models;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace MyStore.Pages.Admin.Companies
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
        public Company Company { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "الرجاء اختيار شعار للشركة")]
        [Display(Name = "شعار الشركة")]
        public IFormFile UploadedLogo { get; set; }

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

            if (UploadedLogo != null)
            {
                // Create a unique file name to avoid conflicts
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(UploadedLogo.FileName);

                // Define the full path to save the image
                string imagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", "logos", fileName);

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

                // Save the file
                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await UploadedLogo.CopyToAsync(fileStream);
                }

                // Save the relative path to the database
                Company.LogoUrl = "/images/logos/" + fileName;
            }

            _context.Companies.Add(Company);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
