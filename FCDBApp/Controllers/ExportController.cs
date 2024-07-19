using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            _logger.LogInformation($"Exporting inspection report for Inspection ID: {inspectionId}");

            var inspection = await _context.InspectionTables
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.InspectionID == inspectionId);

            if (inspection == null)
            {
                _logger.LogWarning($"Inspection with ID {inspectionId} not found.");
                return NotFound();
            }

            _logger.LogInformation($"EngineerSignatureID: {inspection.EngineerSignatureID}");
            _logger.LogInformation($"BranchManagerSignatureID: {inspection.BranchManagerSignatureID}");

            var signatures = await GetSignaturesByIdsAsync(inspection.EngineerSignatureID, inspection.BranchManagerSignatureID);

            var engineerSignature = signatures.FirstOrDefault(s => s.SignatureID == inspection.EngineerSignatureID);
            var branchManagerSignature = signatures.FirstOrDefault(s => s.SignatureID == inspection.BranchManagerSignatureID);

            _logger.LogInformation($"Engineer Signature: ID={engineerSignature?.SignatureID}, Print={engineerSignature?.Print}");
            _logger.LogInformation($"Branch Manager Signature: ID={branchManagerSignature?.SignatureID}, Print={branchManagerSignature?.Print}");

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
                EngineerPrint = engineerSignature?.Print,
                EngineerSignatureImage = engineerSignature?.SignatureImage,
                BranchManagerSignatureID = inspection.BranchManagerSignatureID,
                BranchManagerPrint = branchManagerSignature?.Print,
                BranchManagerSignatureImage = branchManagerSignature?.SignatureImage,
                Details = inspection.Details.Select(d => new InspectionDetailsDto
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList()
            };

            _logger.LogInformation($"Generated InspectionTableDto: EngineerPrint={data.EngineerPrint}, BranchManagerPrint={data.BranchManagerPrint}");

            var filePath = _exportHandler.GenerateInspectionReport(data);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            _logger.LogInformation($"Inspection report generated and saved to {filePath}");

            return File(fileBytes, "application/pdf", System.IO.Path.GetFileName(filePath));
        }

        private async Task<List<Signature>> GetSignaturesByIdsAsync(params Guid?[] signatureIds)
        {
            _logger.LogInformation("Fetching signatures with IDs: {Ids}", string.Join(", ", signatureIds));

            var signatures = await _context.Signatures
                .Where(s => signatureIds.Contains(s.SignatureID))
                .ToListAsync();

            foreach (var signature in signatures)
            {
                _logger.LogInformation("Found signature: ID={ID}, Print={Print}, SignatoryType={SignatoryType}",
                    signature.SignatureID, signature.Print, signature.SignatoryType);
            }

            if (!signatures.Any())
            {
                _logger.LogWarning("No signatures found for the given IDs.");
            }

            return signatures;
        }

        [HttpPost("capture-signature")]
        public async Task<IActionResult> CaptureSignature(
            [FromQuery] Guid[] inspectionIds,
            [FromQuery] Guid[] jobCardIds,
            [FromBody] SignatureDto signatureDto)
        {
            if (signatureDto.SignatureImage == null || string.IsNullOrEmpty(signatureDto.SignatoryType))
            {
                _logger.LogWarning("Invalid signature data provided.");
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

            _logger.LogInformation("Signature captured and associated successfully.");
            return new JsonResult(new { Message = "Signature captured and associated successfully." }) { StatusCode = 200 };
        }
    }
}
