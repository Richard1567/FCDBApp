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
    public class DeleteModel : PageModel
    {
        private readonly FCDBApp.Models.InspectionContext _context;

        public DeleteModel(FCDBApp.Models.InspectionContext context)
        {
            _context = context;
        }

        [BindProperty]
        public JobCard JobCard { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobcard = await _context.JobCards.FirstOrDefaultAsync(m => m.JobCardID == id);

            if (jobcard == null)
            {
                return NotFound();
            }
            else
            {
                JobCard = jobcard;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobcard = await _context.JobCards.FindAsync(id);
            if (jobcard != null)
            {
                JobCard = jobcard;
                _context.JobCards.Remove(JobCard);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
