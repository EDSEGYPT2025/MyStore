using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using MyStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyStore.Pages.Admin.Companies
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly MyStore.Data.ApplicationDbContext _context;

        public IndexModel(MyStore.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Company> Company { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Company = await _context.Companies.ToListAsync();
        }
    }
}
