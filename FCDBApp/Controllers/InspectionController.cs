using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCDBApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InspectionController : ControllerBase
    {
        private readonly InspectionContext _context;
        private readonly ILogger<InspectionController> _logger;

        public InspectionController(InspectionContext context, ILogger<InspectionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Inspection
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InspectionTableDto>>> GetInspectionTables()
        {
            var inspections = await _context.InspectionTables
                                            .Include(i => i.Details)
                                            .ThenInclude(d => d.Item)
                                            .ToListAsync();

            var inspectionDtos = inspections.Select(i => new InspectionTableDto
            {
                InspectionID = i.InspectionID,
                Branch = i.Branch,
                VehicleReg = i.VehicleReg,
                VehicleType = i.VehicleType,
                Odometer = i.Odometer,
                InspectionDate = i.InspectionDate,
                NextInspectionDue = i.NextInspectionDue,
                SubmissionTime = i.SubmissionTime,
                InspectionTypeID = i.InspectionTypeID,
                Details = i.Details.Select(d => new InspectionDetailsDto
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList()
            }).ToList();

            return Ok(inspectionDtos);
        }

        // GET: api/Inspection/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InspectionTableDto>> GetInspection(Guid id)
        {
            var inspection = await _context.InspectionTables
                                           .Include(i => i.Details)
                                           .ThenInclude(d => d.Item)
                                           .FirstOrDefaultAsync(i => i.InspectionID == id);

            if (inspection == null)
            {
                return NotFound();
            }

            var inspectionDto = new InspectionTableDto
            {
                InspectionID = inspection.InspectionID,
                Branch = inspection.Branch,
                VehicleReg = inspection.VehicleReg,
                VehicleType = inspection.VehicleType,
                Odometer = inspection.Odometer,
                InspectionDate = inspection.InspectionDate,
                NextInspectionDue = inspection.NextInspectionDue,
                SubmissionTime = inspection.SubmissionTime,
                InspectionTypeID = inspection.InspectionTypeID,
                Details = inspection.Details.Select(d => new InspectionDetailsDto
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList()
            };

            return Ok(inspectionDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInspection([FromBody] InspectionTable inspection)
        {
            _logger.LogInformation("CreateInspection called");
            _logger.LogInformation("Received Inspection: {@Inspection}", inspection);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid: {ModelState}", ModelState);
                foreach (var state in ModelState)
                {
                    _logger.LogWarning("Key: {Key}, AttemptedValue: {AttemptedValue}, Errors: {Errors}",
                        state.Key,
                        state.Value?.AttemptedValue,
                        string.Join(", ", state.Value?.Errors.Select(e => e.ErrorMessage)));
                }
                return BadRequest(ModelState);
            }

            // Generate a new GUID for the inspection sheet if it's not provided
            if (inspection.InspectionID == Guid.Empty)
            {
                inspection.InspectionID = Guid.NewGuid();
            }

            // Set the submission time
            inspection.SubmissionTime = DateTime.UtcNow; // Ensure this line is present

            // Ensure each detail has the correct InspectionID
            foreach (var detail in inspection.Details)
            {
                detail.InspectionID = inspection.InspectionID;
            }

            _context.InspectionTables.Add(inspection);

            try
            {
                _logger.LogInformation("Attempting to save changes to the database");
                await _context.SaveChangesAsync();
                _logger.LogInformation("Changes saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating Inspection");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the inspection sheet.");
                return BadRequest(ModelState);
            }

            return CreatedAtAction(nameof(GetInspection), new { id = inspection.InspectionID }, inspection);
        }





        // PUT: api/Inspection/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInspection(Guid id, [FromBody] InspectionTableDto inspectionDto)
        {
            _logger.LogInformation("UpdateInspection called with ID: {Id}", id);
            _logger.LogInformation("Received Inspection: {@Inspection}", inspectionDto);

            if (id != inspectionDto.InspectionID)
            {
                _logger.LogWarning("ID mismatch: {Id} != {InspectionID}", id, inspectionDto.InspectionID);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var existingInspection = await _context.InspectionTables
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.InspectionID == id);

            if (existingInspection == null)
            {
                _logger.LogWarning("InspectionTable not found with ID: {Id}", id);
                return NotFound();
            }

            // Update the main entity fields
            existingInspection.Branch = inspectionDto.Branch;
            existingInspection.VehicleReg = inspectionDto.VehicleReg;
            existingInspection.VehicleType = inspectionDto.VehicleType;
            existingInspection.Odometer = inspectionDto.Odometer;
            existingInspection.InspectionDate = inspectionDto.InspectionDate;
            existingInspection.NextInspectionDue = inspectionDto.NextInspectionDue;
            existingInspection.SubmissionTime = DateTime.Now; // Update submission time
            existingInspection.InspectionTypeID = inspectionDto.InspectionTypeID;

            // Update existing details and add new ones
            var incomingDetails = inspectionDto.Details ?? new List<InspectionDetailsDto>();
            var existingDetails = existingInspection.Details.ToList();

            var detailsToRemove = existingDetails.Where(e => !incomingDetails.Any(i => i.InspectionDetailID == e.InspectionDetailID)).ToList();
            _context.InspectionDetails.RemoveRange(detailsToRemove);
            _logger.LogInformation("Details to remove: {@DetailsToRemove}", detailsToRemove);

            foreach (var incomingDetail in incomingDetails)
            {
                var existingDetail = existingDetails.FirstOrDefault(e => e.InspectionDetailID == incomingDetail.InspectionDetailID);
                if (existingDetail != null)
                {
                    existingDetail.Result = incomingDetail.Result;
                    existingDetail.Comments = incomingDetail.Comments;
                    _context.Entry(existingDetail).State = EntityState.Modified;
                    _logger.LogInformation("Updated existing detail: {@ExistingDetail}", existingDetail);
                }
                else
                {
                    var newDetail = new InspectionDetails
                    {
                        InspectionID = inspectionDto.InspectionID,
                        InspectionItemID = incomingDetail.InspectionItemID,
                        Result = incomingDetail.Result,
                        Comments = incomingDetail.Comments
                    };
                    _context.InspectionDetails.Add(newDetail);
                    _logger.LogInformation("Added new detail: {@NewDetail}", newDetail);
                }
            }

            try
            {
                _logger.LogInformation("Attempting to save changes to the database");
                await _context.SaveChangesAsync();
                _logger.LogInformation("Changes saved successfully");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency exception occurred while updating Inspection with ID: {Id}", id);
                var databaseEntity = await _context.InspectionTables.AsNoTracking()
                    .FirstOrDefaultAsync(j => j.InspectionID == id);

                if (databaseEntity == null)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.");
                return Conflict(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating Inspection with ID: {Id}", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the inspection sheet.");
                return BadRequest(ModelState);
            }

            return NoContent();
        }

        // DELETE: api/Inspection/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInspection(Guid id)
        {
            var inspection = await _context.InspectionTables.FindAsync(id);
            if (inspection == null)
            {
                return NotFound();
            }

            _context.InspectionTables.Remove(inspection);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InspectionExists(Guid id)
        {
            return _context.InspectionTables.Any(e => e.InspectionID == id);
        }

    }
}
