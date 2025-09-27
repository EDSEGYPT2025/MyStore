using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyStore.Data;
using MyStore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly MyStore.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CreateModel(MyStore.Data.ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // This holds the list of companies for the dropdown
        public SelectList CompanyNameSL { get; set; }

        public IActionResult OnGet()
        {
            // Populate the dropdown list with companies from the database
            CompanyNameSL = new SelectList(_context.Companies, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Product Product { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "الرجاء اختيار صورة للمنتج")]
        [Display(Name = "صورة المنتج")]
        public IFormFile UploadedImage { get; set; }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // If the model is invalid, we must repopulate the dropdown before returning the page
                CompanyNameSL = new SelectList(_context.Companies, "Id", "Name");
                return Page();
            }

            // Image Upload Logic
            if (UploadedImage != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(UploadedImage.FileName);
                string imagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", "products", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await UploadedImage.CopyToAsync(fileStream);
                }

                Product.ImageUrl = "/images/products/" + fileName;
            }

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
