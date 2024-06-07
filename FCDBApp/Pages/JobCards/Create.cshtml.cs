using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FCDBApp.Models;

namespace FCDBApi.Pages.JobCards
{
    public class CreateModel : PageModel
    {
        private readonly FCDBApp.Models.InspectionContext _context;

        public CreateModel(FCDBApp.Models.InspectionContext context)
        {
            _context = context;
        }

        [BindProperty]
        public JobCard JobCard { get; set; } = new JobCard();

        [BindProperty]
        public List<PartUsed> PartsUsed { get; set; } = new List<PartUsed>();

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

            JobCard.JobCardID = Guid.NewGuid();
            JobCard.SubmissionTime = DateTime.UtcNow;
            JobCard.PartsUsed = PartsUsed;

            _context.JobCards.Add(JobCard);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
