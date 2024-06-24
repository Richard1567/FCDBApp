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
            _logger.LogInformation("OnPostAsync started.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                return Page();
            }

            _logger.LogInformation("Model state is valid. Processing details...");

            var processedDetails = new List<InspectionDetailsDto>();

            foreach (var key in Request.Form.Keys)
            {
                _logger.LogInformation($"Form key: {key}, Value: {Request.Form[key]}");

                if (key.StartsWith("InspectionTable.Details[") && key.EndsWith("].InspectionItemID"))
                {
                    var index = key.Substring(23, key.IndexOf("].InspectionItemID") - 23);
                    var itemID = int.Parse(Request.Form[key]);

                    var resultKey = $"InspectionTable.Details[{index}].Result";
                    var result = Request.Form[resultKey].FirstOrDefault() == "y" ? "y" : "n"; // Assign "y" if checked, otherwise "n"

                    var comments = Request.Form[$"InspectionTable.Details[{index}].Comments"].FirstOrDefault() ?? "";

                    _logger.LogInformation($"Processed detail: ItemID={itemID}, Result={result}, Comments={comments}");

                    processedDetails.Add(new InspectionDetailsDto
                    {
                        InspectionItemID = itemID,
                        Result = result,
                        Comments = comments
                    });
                }
            }

            if (processedDetails.Count == 0)
            {
                _logger.LogWarning("No processed details found.");
            }

            InspectionTable.Details = processedDetails;

            _logger.LogInformation($"InspectionTable ID before update: {InspectionTable.InspectionID}");

            try
            {
                _logger.LogInformation("Calling UpdateInspectionSheetDtoAsync...");
                await _inspectionSheetService.UpdateInspectionSheetDtoAsync(InspectionTable);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Concurrency conflict occurred.");
                return StatusCode(409, "Concurrency conflict occurred. The entity was modified by another user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the inspection sheet.");
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return Page();
            }

            _logger.LogInformation("Inspection sheet updated successfully.");
            return RedirectToPage("./Index");
        }





    }
}
