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
        private readonly InspectionContext _context;

        public EditModel(InspectionSheetService inspectionSheetService, ILogger<EditModel> logger, InspectionContext context)
        {
            _inspectionSheetService = inspectionSheetService;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InspectionTableDto InspectionTable { get; set; }

        public List<InspectionCategoryDto> InspectionCategories { get; set; }
        public List<Site> Sites { get; set; }

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

            InspectionTypeId = inspectionTypeId;
            InspectionCategories = await _inspectionSheetService.GetInspectionCategoriesDtoWithItemsForTypeAsync(InspectionTypeId);
            Sites = await _context.Sites.ToListAsync();

            if (Sites == null || !Sites.Any())
            {
                _logger.LogWarning("No sites found.");
                Sites = new List<Site>();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogWarning($"Model state error: {error.ErrorMessage}");
                        }
                    }
                    return Page();
                }

                string newSiteName = Request.Form["newBranchName"];
                if (!string.IsNullOrWhiteSpace(newSiteName))
                {
                    var newSite = new Site { SiteName = newSiteName };
                    _context.Sites.Add(newSite);
                    await _context.SaveChangesAsync();
                    InspectionTable.SiteID = newSite.SiteID;
                    InspectionTable.Branch = newSiteName;
                    _logger.LogInformation($"New site created with SiteID: {newSite.SiteID}");
                }
                else
                {
                    string branchName = Request.Form["InspectionTable.Branch"];
                    var selectedSite = await _context.Sites.FirstOrDefaultAsync(s => s.SiteName == branchName);
                    if (selectedSite != null)
                    {
                        InspectionTable.SiteID = selectedSite.SiteID;
                        InspectionTable.Branch = branchName;
                        _logger.LogInformation($"Existing site selected with SiteID: {selectedSite.SiteID}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to find SiteID for branch: {branchName}");
                        ModelState.AddModelError("InspectionTable.SiteID", "Invalid Branch.");
                        return Page();
                    }
                }

                // Get the existing inspection table with details
                var inspectionTable = await _context.InspectionTables.Include(it => it.Details).FirstOrDefaultAsync(it => it.InspectionID == InspectionTable.InspectionID);
                if (inspectionTable == null)
                {
                    _logger.LogError($"InspectionTable not found for ID: {InspectionTable.InspectionID}");
                    ModelState.AddModelError(string.Empty, "Inspection not found.");
                    return Page();
                }

                // Map basic fields
                inspectionTable.SiteID = InspectionTable.SiteID;
                inspectionTable.Branch = InspectionTable.Branch;
                inspectionTable.VehicleReg = InspectionTable.VehicleReg;
                inspectionTable.VehicleType = InspectionTable.VehicleType;
                inspectionTable.InspectionDate = InspectionTable.InspectionDate;
                inspectionTable.NextInspectionDue = InspectionTable.NextInspectionDue;
                inspectionTable.PassFailStatus = InspectionTable.PassFailStatus;
                inspectionTable.Odometer = InspectionTable.Odometer;

                // Map details
                var details = new List<InspectionDetails>();
                foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("InspectionTable.Details[")))
                {
                    var itemId = int.Parse(key.Split('[', ']')[1]);
                    var existingDetail = inspectionTable.Details.FirstOrDefault(d => d.InspectionItemID == itemId);

                    if (existingDetail == null)
                    {
                        details.Add(new InspectionDetails
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
                        details.Add(existingDetail);
                    }
                }

                // Remove old details not present in the form
                var detailsList = inspectionTable.Details.ToList();
                detailsList.RemoveAll(d => !details.Any(nd => nd.InspectionItemID == d.InspectionItemID));
                inspectionTable.Details = detailsList;

                // Add new details that are not in the existing list
                foreach (var detail in details)
                {
                    if (!inspectionTable.Details.Any(d => d.InspectionItemID == detail.InspectionItemID))
                    {
                        inspectionTable.Details.Add(detail);
                    }
                }

                _context.Update(inspectionTable);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Inspection sheet updated successfully with SiteID: {inspectionTable.SiteID} and Branch: {inspectionTable.Branch}");

                return RedirectToPage("./Index");
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
