using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCDBApp.Models;
using FCDBApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCDBApi.Pages.InspectionSheets
{
    public class EditModel : PageModel
    {
        private readonly InspectionSheetService _inspectionSheetService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(InspectionSheetService inspectionSheetService, ILogger<EditModel> logger)
        {
            _inspectionSheetService = inspectionSheetService;
            _logger = logger;
        }

        [BindProperty]
        public InspectionTableDto InspectionTable { get; set; }

        public List<InspectionCategoryDto> InspectionCategories { get; set; }

        [BindProperty(SupportsGet = true)]
        public int InspectionTypeId { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id, int inspectionTypeId)
        {
            _logger.LogInformation($"OnGetAsync called with id: {id} and inspectionTypeId: {inspectionTypeId}");

            InspectionTable = await _inspectionSheetService.GetInspectionSheetDtoByIdAsync(id);

            if (InspectionTable == null)
            {
                _logger.LogWarning($"InspectionTable is null for id: {id}");
                return NotFound();
            }

            _logger.LogInformation($"Fetched InspectionTable: ID = {InspectionTable.InspectionID}, InspectionTypeID = {InspectionTable.InspectionTypeID}");

            InspectionTypeId = inspectionTypeId;

            _logger.LogInformation("Fetching InspectionCategories...");
            InspectionCategories = await _inspectionSheetService.GetInspectionCategoriesDtoWithItemsForTypeAsync(InspectionTypeId);

            _logger.LogInformation($"Fetched {InspectionCategories.Count} InspectionCategories");

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

                var details = new List<InspectionDetailsDto>();

                foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("InspectionTable.Details[")))
                {
                    var itemId = int.Parse(key.Split('[', ']')[1]);
                    var existingDetail = details.FirstOrDefault(d => d.InspectionItemID == itemId);

                    if (existingDetail == null)
                    {
                        details.Add(new InspectionDetailsDto
                        {
                            InspectionItemID = itemId,
                            Result = Request.Form[key].FirstOrDefault() ?? "n",
                            Comments = Request.Form[$"InspectionTable.Details[{itemId}].Comments"].FirstOrDefault() ?? string.Empty,
                            InspectionID = InspectionTable.InspectionID
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

                InspectionTable.Details = details;

                _logger.LogInformation($"InspectionTable ID before update: {InspectionTable.InspectionID}");

                await _inspectionSheetService.UpdateInspectionSheetDtoAsync(InspectionTable);

                _logger.LogInformation($"Inspection sheet updated: InspectionID={InspectionTable.InspectionID}");

                return RedirectToPage("./Index");
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "An error occurred while updating the inspection sheet: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the inspection sheet. Please try again.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the inspection sheet.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the inspection sheet. Please try again.");
                return Page();
            }
        }


    }
}
