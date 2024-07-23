using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using FCDBApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FCDBApi.Pages.JobCards
{
    public class CreateModel : PageModel
    {
        private readonly InspectionContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(InspectionContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public JobCard JobCard { get; set; } = new JobCard();

        [BindProperty]
        public List<PartUsed> PartsUsed { get; set; } = new List<PartUsed>();

        [BindProperty]
        public string EngineerPrint { get; set; } = string.Empty;

        [BindProperty]
        public string EngineerSignature { get; set; } = string.Empty;

        [BindProperty]
        public string BranchManagerPrint { get; set; } = string.Empty;

        [BindProperty]
        public string BranchManagerSignature { get; set; } = string.Empty;

        public List<PartsList> PartsList { get; set; } = new List<PartsList>();

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Job Card creation page accessed.");
            PartsList = await _context.PartsList.ToListAsync(); // Fetch the parts list
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Job Card creation process started.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogError($"Error in field '{state.Key}': {error.ErrorMessage}");
                        }
                    }
                }
                return Page();
            }

            JobCard.JobCardID = Guid.NewGuid();
            JobCard.SubmissionTime = DateTime.UtcNow;
            JobCard.PartsUsed = PartsUsed;
            JobCard.Time = CalculateTimeDifference(JobCard.StartTime, JobCard.EndTime);
            _logger.LogInformation($"JobCardID: {JobCard.JobCardID}, SubmissionTime: {JobCard.SubmissionTime}, Time: {JobCard.Time}");

            if (!string.IsNullOrEmpty(EngineerSignature))
            {
                try
                {
                    var engineerSignature = new Signature
                    {
                        SignatureID = Guid.NewGuid(),
                        SignatureImage = Convert.FromBase64String(EngineerSignature.Split(",")[1]),
                        SignatoryType = "Engineer",
                        Print = EngineerPrint,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Signatures.Add(engineerSignature);
                    JobCard.EngineerSignatureID = engineerSignature.SignatureID;

                    _logger.LogInformation($"Engineer Signature added with ID: {engineerSignature.SignatureID}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error adding Engineer Signature: {ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(BranchManagerSignature))
            {
                try
                {
                    var branchManagerSignature = new Signature
                    {
                        SignatureID = Guid.NewGuid(),
                        SignatureImage = Convert.FromBase64String(BranchManagerSignature.Split(",")[1]),
                        SignatoryType = "BranchManager",
                        Print = BranchManagerPrint,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Signatures.Add(branchManagerSignature);
                    JobCard.BranchManagerSignatureID = branchManagerSignature.SignatureID;

                    _logger.LogInformation($"Branch Manager Signature added with ID: {branchManagerSignature.SignatureID}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error adding Branch Manager Signature: {ex.Message}");
                }
            }

            _context.JobCards.Add(JobCard);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Job Card saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving Job Card: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the Job Card.");
                return Page();
            }

            return RedirectToPage("./Index");
        }

        private string CalculateTimeDifference(string startTime, string endTime)
        {
            if (TimeSpan.TryParse(startTime, out var start) && TimeSpan.TryParse(endTime, out var end))
            {
                var difference = end - start;
                return difference.ToString(@"hh\:mm");
            }
            return "00:00";
        }
    }
}
