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
    public class DeleteModel : PageModel
    {
        private readonly FCDBApp.Models.InspectionContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(FCDBApp.Models.InspectionContext context, ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public JobCard JobCard { get; set; } = default!;

        public string EngineerPrint { get; set; }
        public byte[] EngineerSignature { get; set; }
        public string BranchManagerPrint { get; set; }
        public byte[] BranchManagerSignature { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobcard = await _context.JobCards
                .Include(j => j.PartsUsed)
                .FirstOrDefaultAsync(m => m.JobCardID == id);

            if (jobcard == null)
            {
                return NotFound();
            }

            JobCard = jobcard;

            if (jobcard.EngineerSignatureID.HasValue)
            {
                var engineerSignature = await _context.Signatures
                    .FirstOrDefaultAsync(s => s.SignatureID == jobcard.EngineerSignatureID);
                if (engineerSignature != null)
                {
                    EngineerPrint = engineerSignature.Print;
                    EngineerSignature = engineerSignature.SignatureImage;
                }
            }

            if (jobcard.BranchManagerSignatureID.HasValue)
            {
                var branchManagerSignature = await _context.Signatures
                    .FirstOrDefaultAsync(s => s.SignatureID == jobcard.BranchManagerSignatureID);
                if (branchManagerSignature != null)
                {
                    BranchManagerPrint = branchManagerSignature.Print;
                    BranchManagerSignature = branchManagerSignature.SignatureImage;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobcard = await _context.JobCards
                .Include(j => j.PartsUsed) // Include PartsUsed for deletion
                .FirstOrDefaultAsync(m => m.JobCardID == id);

            if (jobcard == null)
            {
                return NotFound();
            }

            // Delete associated PartsUsed
            _context.PartsUsed.RemoveRange(jobcard.PartsUsed);
            _context.JobCards.Remove(jobcard);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Job Card and associated PartsUsed deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting Job Card: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the Job Card.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
