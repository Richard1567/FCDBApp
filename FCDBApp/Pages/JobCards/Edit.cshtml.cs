using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FCDBApp.Models;
using Microsoft.Extensions.Logging;

namespace FCDBApi.Pages.JobCards
{
    public class EditModel : PageModel
    {
        private readonly FCDBApp.Models.InspectionContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(FCDBApp.Models.InspectionContext context, ILogger<EditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPostAsync called");
            _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    _logger.LogWarning("Key: {Key}, AttemptedValue: {AttemptedValue}, Errors: {Errors}",
                        state.Key,
                        state.Value?.AttemptedValue,
                        string.Join(", ", state.Value?.Errors.Select(e => e.ErrorMessage)));
                }
                return Page();
            }

            _context.Attach(JobCard).State = EntityState.Modified;

            // Handle the PartsUsed list separately to ensure changes are tracked correctly
            var existingParts = await _context.PartsUsed.Where(p => p.JobCardID == JobCard.JobCardID).ToListAsync();
            _context.PartsUsed.RemoveRange(existingParts);

            if (JobCard.PartsUsed != null)
            {
                foreach (var part in JobCard.PartsUsed)
                {
                    part.JobCardID = JobCard.JobCardID;
                    _context.PartsUsed.Add(part);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobCardExists(JobCard.JobCardID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool JobCardExists(Guid id)
        {
            return _context.JobCards.Any(e => e.JobCardID == id);
        }
    }
}
