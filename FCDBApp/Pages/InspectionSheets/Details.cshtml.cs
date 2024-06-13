using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCDBApp.Models;
using FCDBApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

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

        public InspectionTableDto InspectionTable { get; set; }
        public List<InspectionCategoryDto> CategoriesWithItems { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            _logger.LogInformation($"Fetching inspection sheet with ID: {id}");

            InspectionTable = await _inspectionSheetService.GetInspectionSheetByIdAsync(id);

            if (InspectionTable == null)
            {
                _logger.LogWarning($"Inspection sheet with ID: {id} not found.");
                return NotFound();
            }

            _logger.LogInformation($"Inspection sheet found: {InspectionTable}");

            CategoriesWithItems = await _inspectionSheetService.GetInspectionCategoriesDtoWithItemsForTypeAsync(InspectionTable.InspectionTypeID);

            _logger.LogInformation($"Categories with items: {string.Join(", ", CategoriesWithItems.Select(c => c.CategoryName))}");

            return Page();
        }
    }
}
