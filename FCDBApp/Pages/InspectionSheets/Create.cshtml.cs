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
    public class CreateModel : PageModel
    {
        private readonly InspectionSheetService _inspectionSheetService;
        private readonly ILogger<CreateModel> _logger;
        private readonly InspectionContext _context;

        public CreateModel(InspectionSheetService inspectionSheetService, ILogger<CreateModel> logger, InspectionContext context)
        {
            _inspectionSheetService = inspectionSheetService;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InspectionTable InspectionTable { get; set; }

        public List<InspectionCategories> InspectionCategories { get; set; }
        public List<Site> Sites { get; set; }

        [BindProperty(SupportsGet = true)]
        public int InspectionTypeId { get; set; }

        public async Task<IActionResult> OnGetAsync(int inspectionTypeId)
        {
            _logger.LogInformation($"OnGetAsync called with Inspection Type ID: {inspectionTypeId}");
            InspectionTypeId = inspectionTypeId;

            InspectionCategories = await _inspectionSheetService.GetInspectionCategoriesWithItemsForTypeAsync(InspectionTypeId);
            Sites = await _context.Sites.ToListAsync();

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

                    foreach (var modelState in ModelState)
                    {
                        foreach (var error in modelState.Value.Errors)
                        {
                            _logger.LogError($"Key: {modelState.Key}, Error: {error.ErrorMessage}");
                        }
                    }

                    return Page();
                }
                _logger.LogInformation($"Form submission time: {Request.Form["InspectionTable.SubmissionTime"]}");
                InspectionTable.InspectionID = Guid.NewGuid();
                _logger.LogInformation($"Generated new InspectionID: {InspectionTable.InspectionID}");

                var branch = Request.Form["InspectionTable.Branch"].FirstOrDefault();
                if (branch == "AddNew")
                {
                    var newBranchName = Request.Form["newBranchName"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(newBranchName))
                    {
                        var newSite = new Site { SiteName = newBranchName };
                        _context.Sites.Add(newSite);
                        await _context.SaveChangesAsync();
                        InspectionTable.SiteID = newSite.SiteID;
                        InspectionTable.Branch = newBranchName;
                    }
                    else
                    {
                        ModelState.AddModelError("InspectionTable.Branch", "New branch name cannot be empty.");
                        return Page();
                    }
                }
                else
                {
                    var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteName == branch);
                    if (site == null)
                    {
                        throw new Exception($"Site not found for Branch: {branch}");
                    }
                    InspectionTable.SiteID = site.SiteID;
                    InspectionTable.Branch = branch;
                }

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
                InspectionTable.PassFailStatus = Request.Form["InspectionTable.PassFailStatus"].FirstOrDefault();

                // Save engineer and branch manager signatures and prints
                var engineerSignature = Request.Form["EngineerSignature"].FirstOrDefault();
                var branchManagerSignature = Request.Form["BranchManagerSignature"].FirstOrDefault();
                var engineerPrint = Request.Form["EngineerPrint"].FirstOrDefault();
                var branchManagerPrint = Request.Form["BranchManagerPrint"].FirstOrDefault();

                if (!string.IsNullOrEmpty(engineerSignature))
                {
                    var engineerSignatureEntity = new Signature
                    {
                        SignatureID = Guid.NewGuid(),
                        SignatureImage = Convert.FromBase64String(engineerSignature.Split(",")[1]), // Remove the data:image/png;base64, part
                        SignatoryType = "Engineer",
                        Print = engineerPrint,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Signatures.Add(engineerSignatureEntity);
                    InspectionTable.EngineerSignatureID = engineerSignatureEntity.SignatureID;
                }

                if (!string.IsNullOrEmpty(branchManagerSignature))
                {
                    var branchManagerSignatureEntity = new Signature
                    {
                        SignatureID = Guid.NewGuid(),
                        SignatureImage = Convert.FromBase64String(branchManagerSignature.Split(",")[1]), // Remove the data:image/png;base64, part
                        SignatoryType = "BranchManager",
                        Print = branchManagerPrint,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Signatures.Add(branchManagerSignatureEntity);
                    InspectionTable.BranchManagerSignatureID = branchManagerSignatureEntity.SignatureID;
                }

                // Set the SubmissionTime property
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
