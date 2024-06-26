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
        public InspectionTable InspectionTable { get; set; }

        [BindProperty(SupportsGet = true)]
        public int InspectionTypeId { get; set; }
        public List<InspectionCategories> CategoriesWithItems { get; set; }
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

            return Page();
        }


        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            _logger.LogInformation($"Deleting inspection sheet with ID: {id}");

            await _inspectionSheetService.DeleteInspectionSheetAsync(id);

            _logger.LogInformation($"Inspection sheet with ID: {id} deleted successfully.");

            return RedirectToPage("./Index");
        }
    }
}
