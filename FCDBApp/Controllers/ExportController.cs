using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FCDBApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly InspectionContext _context;
        private readonly ExportHandler _exportHandler;
        private readonly ILogger<ExportController> _logger;

        public ExportController(InspectionContext context, ExportHandler exportHandler, ILogger<ExportController> logger)
        {
            _context = context;
            _exportHandler = exportHandler;
            _logger = logger;
        }

        [HttpPost("export/{inspectionId}")]
        public async Task<IActionResult> ExportInspection(Guid inspectionId)
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
                Odometer = inspection.Odometer,
                InspectionDate = inspection.InspectionDate,
                NextInspectionDue = inspection.NextInspectionDue,
                PassFailStatus = inspection.PassFailStatus,
                InspectionTypeID = inspection.InspectionTypeID,
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
    }
}
