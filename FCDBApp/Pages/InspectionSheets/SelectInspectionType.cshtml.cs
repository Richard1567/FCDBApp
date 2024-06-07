using FCDBApp.Models;
using FCDBApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; // Add this using directive
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FCDBApi.Pages.InspectionSheets
{
    public class SelectInspectionTypeModel : PageModel
    {
        private readonly InspectionSheetService _inspectionSheetService;
        private readonly ILogger<SelectInspectionTypeModel> _logger; // Add logger

        public SelectInspectionTypeModel(InspectionSheetService inspectionSheetService, ILogger<SelectInspectionTypeModel> logger)
        {
            _inspectionSheetService = inspectionSheetService;
            _logger = logger; // Initialize logger
        }

        public List<InspectionType> InspectionTypes { get; set; }
        [BindProperty]
        public int SelectedInspectionType { get; set; } // Add BindProperty for selected inspection type

        public async Task OnGetAsync()
        {
            InspectionTypes = (await _inspectionSheetService.GetInspectionTypesAsync()).ToList();
        }

        public IActionResult OnPost()
        {
            // Log the selected inspection type
            _logger.LogInformation("Selected Inspection Type ID: {InspectionTypeId}", SelectedInspectionType);

            return RedirectToPage("./Create", new { inspectionTypeId = SelectedInspectionType });
        }
    }
}
