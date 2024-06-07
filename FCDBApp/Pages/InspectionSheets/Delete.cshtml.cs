using FCDBApp.Models;
using FCDBApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FCDBApi.Pages.InspectionSheets
{
    public class DeleteModel : PageModel
    {
        private readonly InspectionSheetService _inspectionSheetService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(InspectionSheetService inspectionSheetService, ILogger<DeleteModel> logger)
        {
            _inspectionSheetService = inspectionSheetService;
            _logger = logger;
        }

        [BindProperty]
        public InspectionTableDto InspectionTable { get; set; }

        [BindProperty(SupportsGet = true)]
        public int InspectionTypeId { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id, int inspectionTypeId)
        {
            _logger.LogInformation($"OnGetAsync called with id: {id} and inspectionTypeId: {inspectionTypeId}");

            InspectionTypeId = inspectionTypeId;

            _logger.LogInformation("Fetching InspectionTable...");
            InspectionTable = await _inspectionSheetService.GetInspectionSheetByIdAsync(id);

            if (InspectionTable == null)
            {
                _logger.LogWarning("InspectionTable is null");
                return RedirectToPage("./Index");
            }

            if (InspectionTable.InspectionTypeID != InspectionTypeId)
            {
                _logger.LogWarning("InspectionTypeID does not match");
                return RedirectToPage("./Index");
            }

            _logger.LogInformation("InspectionTable fetched successfully");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (InspectionTable == null || InspectionTable.InspectionID == Guid.Empty)
            {
                return NotFound();
            }

            await _inspectionSheetService.DeleteInspectionSheetAsync(InspectionTable.InspectionID);

            return RedirectToPage("./Index");
        }
    }
}
