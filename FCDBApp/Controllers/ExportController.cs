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
                .Include(i => i.EngineerSignature)
                .Include(i => i.BranchManagerSignature)
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
                EngineerSignatureID = inspection.EngineerSignatureID,
                EngineerPrint = inspection.EngineerSignature?.Print,
                EngineerSignatureImage = inspection.EngineerSignature?.SignatureImage,
                BranchManagerSignatureID = inspection.BranchManagerSignatureID,
                BranchManagerPrint = inspection.BranchManagerSignature?.Print,
                BranchManagerSignatureImage = inspection.BranchManagerSignature?.SignatureImage,
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



        [HttpPost("capture-signature")]
        public async Task<IActionResult> CaptureSignature(
            [FromQuery] Guid[] inspectionIds,
            [FromQuery] Guid[] jobCardIds,
            [FromBody] SignatureDto signatureDto)
        {
            if (signatureDto.SignatureImage == null || string.IsNullOrEmpty(signatureDto.SignatoryType))
            {
                return new JsonResult(new { Message = "Invalid signature data." }) { StatusCode = 400 };
            }

            var signature = new Signature
            {
                SignatureID = Guid.NewGuid(),
                SignatureImage = signatureDto.SignatureImage,
                SignatoryType = signatureDto.SignatoryType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Signatures.Add(signature);

            foreach (var inspectionId in inspectionIds)
            {
                var inspection = await _context.InspectionTables.FindAsync(inspectionId);
                if (inspection != null)
                {
                    if (signature.SignatoryType == "Engineer")
                    {
                        inspection.EngineerSignatureID = signature.SignatureID;
                    }
                    else if (signature.SignatoryType == "BranchManager")
                    {
                        inspection.BranchManagerSignatureID = signature.SignatureID;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new JsonResult(new { Message = "Signature captured and associated successfully." }) { StatusCode = 200 };
        }
    }
}
