using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly MyStore.Data.ApplicationDbContext _context;

        public IndexModel(MyStore.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Product> Product { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Products != null)
            {
                // We use .Include() to also load the related Company information
                // This is crucial to display the company name in the table
                Product = await _context.Products
                    .Include(p => p.Company)
                    .ToListAsync();
            }
        }
    }
}
