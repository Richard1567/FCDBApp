using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCDBApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FCDBApi.Pages.InspectionSheets
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly InspectionContext _context;
        private readonly ExportHandler _exportHandler;

        public IndexModel(InspectionContext context, ExportHandler exportHandler)
        {
            _context = context;
            _exportHandler = exportHandler;
        }

        public IList<InspectionTableDto> InspectionTable { get; set; }
        public Dictionary<int, string> SiteNames { get; set; }

        public async Task OnGetAsync()
        {
            InspectionTable = await _context.InspectionTables
                .Select(i => new InspectionTableDto
                {
                    InspectionID = i.InspectionID,
                    Branch = i.Branch,
                    VehicleReg = i.VehicleReg,
                    VehicleType = i.VehicleType,
                    Odometer = i.Odometer,
                    InspectionDate = i.InspectionDate,
                    NextInspectionDue = i.NextInspectionDue,
                    PassFailStatus = i.PassFailStatus,
                    InspectionTypeID = i.InspectionTypeID,
                    SiteID = i.SiteID,
                    Details = i.Details.Select(d => new InspectionDetailsDto
                    {
                        InspectionDetailID = d.InspectionDetailID,
                        InspectionID = d.InspectionID,
                        InspectionItemID = d.InspectionItemID,
                        Result = d.Result,
                        Comments = d.Comments
                    }).ToList()
                })
                .ToListAsync();

            SiteNames = await _context.Sites.ToDictionaryAsync(s => s.SiteID, s => s.SiteName);
        }

        public async Task<IActionResult> OnPostExportAsync(Guid inspectionId)
        {
            var inspection = await _context.InspectionTables
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.InspectionID == inspectionId);

            if (inspection == null)
            {
                return NotFound();
            }

            var data = new InspectionTableDto
            {
                InspectionID = inspection.InspectionID,
                Branch = inspection.Branch,
                VehicleReg = inspection.VehicleReg,
                VehicleType = inspection.VehicleType,
                InspectionDate = inspection.InspectionDate,
                NextInspectionDue = inspection.NextInspectionDue,
                Odometer = inspection.Odometer,
                InspectionTypeID = inspection.InspectionTypeID,
                PassFailStatus = inspection.PassFailStatus,
                SiteID = inspection.SiteID,
                Details = inspection.Details.Select(d => new InspectionDetailsDto
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList()
            };

            var filePath = _exportHandler.GenerateInspectionReport(data);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            return File(fileBytes, "application/pdf", Path.GetFileName(filePath));
        }

        private static string CombineNotes(ICollection<InspectionDetailsDto> details)
        {
            return string.Join("; ", details.Where(d => !string.IsNullOrWhiteSpace(d.Comments)).Select(d => d.Comments));
        }
    }
}
