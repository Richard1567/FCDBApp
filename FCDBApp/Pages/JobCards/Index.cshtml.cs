using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FCDBApp.Models;

namespace FCDBApi.Pages.JobCards
{
    public class IndexModel : PageModel
    {
        private readonly FCDBApp.Models.InspectionContext _context;

        public IndexModel(FCDBApp.Models.InspectionContext context)
        {
            _context = context;
        }

        public IList<JobCard> JobCard { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.JobCards != null)
            {
                JobCard = await _context.JobCards
                    .Include(j => j.PartsUsed)
                    .ToListAsync();
            }
        }
    }
}
