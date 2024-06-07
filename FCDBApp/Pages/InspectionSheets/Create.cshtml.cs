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
    public class CreateModel : PageModel
    {
        private readonly InspectionSheetService _inspectionSheetService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(InspectionSheetService inspectionSheetService, ILogger<CreateModel> logger)
        {
            _inspectionSheetService = inspectionSheetService;
            _logger = logger;
        }

        [BindProperty]
        public InspectionTableDto InspectionTable { get; set; }

        public List<InspectionCategoryDto> InspectionCategories { get; set; }

        [BindProperty(SupportsGet = true)]
        public int InspectionTypeId { get; set; }

        public async Task<IActionResult> OnGetAsync(int inspectionTypeId)
        {
            _logger.LogInformation($"OnGetAsync called with Inspection Type ID: {inspectionTypeId}");
            InspectionTypeId = inspectionTypeId;

            InspectionCategories = await _inspectionSheetService.GetInspectionCategoriesWithItemsForTypeAsync(InspectionTypeId);

            if (InspectionCategories == null || !InspectionCategories.Any())
            {
                _logger.LogWarning("InspectionCategories is null or empty.");
                return NotFound();
            }

            _logger.LogInformation($"Fetched {InspectionCategories.Count} categories");

            InspectionTable = new InspectionTableDto
            {
                InspectionTypeID = InspectionTypeId
            };

            _logger.LogInformation("InspectionTable created successfully.");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                _logger.LogInformation($"Creating new InspectionTable with ID: {InspectionTable.InspectionID}");

                // Re-fetch InspectionCategories on POST
                InspectionCategories = await _inspectionSheetService.GetInspectionCategoriesWithItemsForTypeAsync(InspectionTypeId);
                if (InspectionCategories == null)
                {
                    _logger.LogError("InspectionCategories is null in OnPostAsync.");
                    throw new ArgumentNullException(nameof(InspectionCategories), "InspectionCategories is null.");
                }

                var details = InspectionCategories.SelectMany(c => c.Items
                    .Where(i => i.InspectionTypeID == InspectionTypeId)
                    .Select(i => new InspectionDetailsDto
                    {
                        InspectionItemID = i.InspectionItemID,
                        Result = Request.Form[$"InspectionTable.Details[{i.InspectionItemID}].Result"].FirstOrDefault() ?? "n",
                        Comments = Request.Form[$"InspectionTable.Details[{i.InspectionItemID}].Comments"].FirstOrDefault() ?? string.Empty
                    })).ToList();

                _logger.LogInformation($"Details after form binding: {string.Join(", ", details.Select(d => $"ItemID={d.InspectionItemID}, Result={d.Result}, Comments={d.Comments}"))}");

                InspectionTable.Details = details;

                // Set SubmissionTime
                InspectionTable.SubmissionTime = DateTime.Now;

                await _inspectionSheetService.CreateInspectionSheetAsync(InspectionTable);

                _logger.LogInformation($"Inspection sheet created: InspectionID={InspectionTable.InspectionID}");

                return RedirectToPage("./Index");
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "An error occurred while creating the inspection sheet: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred while creating the inspection sheet. Please try again.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating the inspection sheet.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the inspection sheet. Please try again.");
                return Page();
            }
        }
    }
}
