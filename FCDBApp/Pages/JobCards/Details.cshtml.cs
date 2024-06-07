using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FCDBApp.Models;

namespace FCDBApi.Pages.JobCards
{
    public class DetailsModel : PageModel
    {
        private readonly FCDBApp.Models.InspectionContext _context;

        public DetailsModel(FCDBApp.Models.InspectionContext context)
        {
            _context = context;
        }

        public JobCard JobCard { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobcard = await _context.JobCards
                .Include(j => j.PartsUsed) // Include PartsUsed list
                .FirstOrDefaultAsync(m => m.JobCardID == id);

            if (jobcard == null)
            {
                return NotFound();
            }

            JobCard = jobcard;
            return Page();
        }
    }
}
