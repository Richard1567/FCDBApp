using FCDBApp.Models;
using FCDBApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FCDBApi.Pages.InspectionSheets
{
    public class DetailsModel : PageModel
    {
        private readonly InspectionSheetService _inspectionSheetService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(InspectionSheetService inspectionSheetService, ILogger<DetailsModel> logger)
        {
            _inspectionSheetService = inspectionSheetService;
            _logger = logger;
        }

        public InspectionTable InspectionTable { get; set; }
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

            CategoriesWithItems = await _inspectionSheetService.GetInspectionCategoriesWithItemsForTypeAsync(inspectionTypeId);

            _logger.LogInformation($"Categories with items: {string.Join(", ", CategoriesWithItems.Select(c => c.CategoryName))}");

            return Page();
        }
    }
}
