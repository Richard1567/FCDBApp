using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCDBApi.Pages.PartsUsed
{
    public class ManagePartsUsedModel : PageModel
    {
        private readonly InspectionContext _context;
        private readonly ILogger<ManagePartsUsedModel> _logger;

        public ManagePartsUsedModel(InspectionContext context, ILogger<ManagePartsUsedModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<ManagePartsUsedViewModel> PartsUsedByEngineer { get; set; } = new List<ManagePartsUsedViewModel>();

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Fetching parts used by engineer.");

            var partsUsed = await _context.PartsUsed
                .ToListAsync();

            var jobCardIds = partsUsed.Select(p => p.JobCardID).Distinct().ToList();
            var jobCards = await _context.JobCards
                .Where(j => jobCardIds.Contains(j.JobCardID))
                .ToDictionaryAsync(j => j.JobCardID);

            PartsUsedByEngineer = partsUsed
                .GroupBy(p => jobCards[p.JobCardID].Engineer)
                .Select(g => new ManagePartsUsedViewModel
                {
                    Engineer = g.Key,
                    PartsUsed = g.Select(p => new PartUsedDetails
                    {
                        PartName = p.Name,
                        PartNumber = p.PartNumber,
                        Quantity = p.Quantity,
                        JobRegNo = jobCards[p.JobCardID].RegNo,
                        JobDate = jobCards[p.JobCardID].Date
                    }).ToList()
                }).ToList();

            _logger.LogInformation("Parts used by engineer fetched successfully.");
        }
    }
}
