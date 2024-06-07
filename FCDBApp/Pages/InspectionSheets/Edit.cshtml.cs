using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCDBApp.Models;
using FCDBApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FCDBApi.Pages.InspectionSheets
{
    public class EditModel : PageModel
    {
        private readonly InspectionSheetService _inspectionSheetService;

        public EditModel(InspectionSheetService inspectionSheetService)
        {
            _inspectionSheetService = inspectionSheetService;
        }

        [BindProperty]
        public InspectionTableDto InspectionTable { get; set; }

        public List<InspectionCategoryDto> InspectionCategories { get; set; }

        [BindProperty(SupportsGet = true)]
        public int InspectionTypeId { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            InspectionTable = await _inspectionSheetService.GetInspectionSheetByIdAsync(id);

            if (InspectionTable == null)
            {
                return NotFound();
            }

            InspectionTypeId = InspectionTable.InspectionTypeID;

            InspectionCategories = await _inspectionSheetService.GetInspectionCategoriesWithItemsForTypeAsync(InspectionTypeId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            foreach (var detail in InspectionTable.Details)
            {
                var result = Request.Form[$"InspectionTable.Details[{detail.InspectionItemID}].Result"].FirstOrDefault() ?? "n";
                detail.Result = result;

                var comments = Request.Form[$"InspectionTable.Details[{detail.InspectionItemID}].Comments"].FirstOrDefault();
                detail.Comments = comments;
            }

            try
            {
                await _inspectionSheetService.UpdateInspectionSheetAsync(InspectionTable);
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(409, "Concurrency conflict occurred. The entity was modified by another user.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
