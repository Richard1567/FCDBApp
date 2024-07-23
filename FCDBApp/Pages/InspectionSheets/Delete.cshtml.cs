using FCDBApp.Models;
using FCDBApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FCDBApi.Pages.InspectionSheets
{
    public class DeleteModel : PageModel
    {
        private readonly InspectionSheetService _inspectionSheetService;
        private readonly ILogger<DeleteModel> _logger;
        private readonly InspectionContext _context;

        public DeleteModel(InspectionSheetService inspectionSheetService, ILogger<DeleteModel> logger, InspectionContext context)
        {
            _inspectionSheetService = inspectionSheetService;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InspectionTable InspectionTable { get; set; }

        [BindProperty(SupportsGet = true)]
        public int InspectionTypeId { get; set; }
        public List<InspectionCategories> CategoriesWithItems { get; set; }
        public SignatureDto EngineerSignature { get; set; }
        public SignatureDto BranchManagerSignature { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id, int inspectionTypeId)
        {
            _logger.LogInformation($"Fetching inspection sheet with ID: {id}");

            InspectionTable = await _inspectionSheetService.GetInspectionSheetByIdAsync(id);

            if (InspectionTable == null)
            {
                _logger.LogWarning($"Inspection sheet with ID: {id} not found.");
                return NotFound();
            }

            InspectionTypeId = inspectionTypeId;

            // Fetch categories with items
            CategoriesWithItems = await _inspectionSheetService.GetInspectionCategoriesWithItemsForTypeAsync(inspectionTypeId);

            _logger.LogInformation($"Categories with items: {string.Join(", ", CategoriesWithItems.Select(c => c.CategoryName))}");

            EngineerSignature = await FetchSignatureAsync(InspectionTable.EngineerSignatureID);
            BranchManagerSignature = await FetchSignatureAsync(InspectionTable.BranchManagerSignatureID);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            _logger.LogInformation($"Deleting inspection sheet with ID: {id}");

            var inspectionSheet = await _context.InspectionTables
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.InspectionID == id);

            if (inspectionSheet == null)
            {
                _logger.LogWarning($"Inspection sheet with ID: {id} not found.");
                return NotFound();
            }

            // Remove related inspection details
            _context.InspectionDetails.RemoveRange(inspectionSheet.Details);

            // Remove inspection sheet
            _context.InspectionTables.Remove(inspectionSheet);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Inspection sheet with ID: {id} and its details deleted successfully.");

            return RedirectToPage("./Index");
        }

        private async Task<SignatureDto> FetchSignatureAsync(Guid? signatureId)
        {
            if (signatureId == null)
            {
                return null;
            }

            var signature = await _context.Signatures
                .Where(s => s.SignatureID == signatureId)
                .Select(s => new SignatureDto
                {
                    SignatureID = s.SignatureID,
                    Print = s.Print,
                    SignatureImage = s.SignatureImage,
                    SignatoryType = s.SignatoryType
                })
                .FirstOrDefaultAsync();

            return signature;
        }
    }
}
