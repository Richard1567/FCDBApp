using System;
using System.Linq;
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
    }
}
