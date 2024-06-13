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
        public InspectionTable InspectionTable { get; set; }

        public List<InspectionCategories> InspectionCategories { get; set; }

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

            InspectionTable = new InspectionTable
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
                    _logger.LogWarning("Model state is invalid.");
                    return Page();
                }

                var inspectionID = Guid.NewGuid();
                _logger.LogInformation($"Generated new InspectionID: {inspectionID}");

                var details = new List<InspectionDetails>();

                foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("InspectionTable.Details[")))
                {
                    var itemId = int.Parse(key.Split('[', ']')[1]);
                    var existingDetail = details.FirstOrDefault(d => d.InspectionItemID == itemId);

                    if (existingDetail == null)
                    {
                        details.Add(new InspectionDetails
                        {
                            InspectionItemID = itemId,
                            Result = Request.Form[key].FirstOrDefault() ?? "n",
                            Comments = Request.Form[$"InspectionTable.Details[{itemId}].Comments"].FirstOrDefault() ?? string.Empty,
                            InspectionID = inspectionID
                        });
                    }
                    else
                    {
                        if (key.EndsWith(".Result"))
                        {
                            existingDetail.Result = Request.Form[key].FirstOrDefault() ?? "n";
                        }
                        else if (key.EndsWith(".Comments"))
                        {
                            existingDetail.Comments = Request.Form[key].FirstOrDefault() ?? string.Empty;
                        }
                    }
                }

                if (details.Any())
                {
                    foreach (var detail in details)
                    {
                        _logger.LogInformation($"ItemID={detail.InspectionItemID}, Result={detail.Result}, Comments={detail.Comments}");
                    }
                }
                else
                {
                    _logger.LogWarning("No details found after form binding.");
                }

                InspectionTable.InspectionID = inspectionID;
                InspectionTable.Details = details;

                await _inspectionSheetService.CreateInspectionSheetAsync(InspectionTable);

                _logger.LogInformation($"Inspection sheet created: InspectionID={inspectionID}");

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
