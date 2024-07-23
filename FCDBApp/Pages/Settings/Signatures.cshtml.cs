using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FCDBApi.Pages
{
    public class SignaturesModel : PageModel
    {
        private readonly InspectionContext _context;
        private readonly ILogger<SignaturesModel> _logger;

        public SignaturesModel(InspectionContext context, ILogger<SignaturesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IList<SignatureDto> Signatures { get; set; }

        public async Task OnGetAsync()
        {
            Signatures = await _context.Signatures
                .Select(s => new SignatureDto
                {
                    SignatureID = s.SignatureID,
                    Print = s.Print,
                    SignatureImage = s.SignatureImage,
                    SignatoryType = s.SignatoryType
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var signature = await _context.Signatures.FindAsync(id);

            if (signature == null)
            {
                _logger.LogWarning($"Signature with ID: {id} not found.");
                return NotFound();
            }

            // Check if the signature is used by any inspection sheets
            var isUsedByInspections = await _context.InspectionTables
                .AnyAsync(i => i.EngineerSignatureID == id || i.BranchManagerSignatureID == id);

            if (isUsedByInspections)
            {
                _logger.LogWarning($"Signature with ID: {id} is used by one or more inspection sheets and cannot be deleted.");
                return BadRequest("Signature is used by one or more inspection sheets and cannot be deleted.");
            }

            _context.Signatures.Remove(signature);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Signature with ID: {id} deleted successfully.");
            return RedirectToPage();
        }
    }
}
