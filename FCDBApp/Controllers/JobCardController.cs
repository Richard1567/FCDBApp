using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FCDBApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FCDBApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobCardController : ControllerBase
    {
        private readonly InspectionContext _context;
        private readonly ILogger<JobCardController> _logger;

        public JobCardController(InspectionContext context, ILogger<JobCardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/JobCard
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobCard>>> GetJobCards()
        {
            return await _context.JobCards.Include(j => j.PartsUsed).ToListAsync();
        }

        // GET: api/JobCard/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<JobCard>> GetJobCard(Guid id)
        {
            var jobCard = await _context.JobCards
                .Include(j => j.PartsUsed)
                .FirstOrDefaultAsync(j => j.JobCardID == id);

            if (jobCard == null)
            {
                return NotFound();
            }

            return jobCard;
        }

        // POST: api/JobCard
        [HttpPost]
        public async Task<ActionResult<JobCard>> CreateJobCard(JobCard jobCard)
        {
            if (ModelState.IsValid)
            {
                jobCard.JobCardID = Guid.NewGuid();
                jobCard.SubmissionTime = DateTime.UtcNow; // Set the server submission time

                // The RowVersion will be set by the database on insert
                _context.JobCards.Add(jobCard);
                await _context.SaveChangesAsync();

                // Retrieve the jobCard from the database to get the initialized RowVersion
                var createdJobCard = await _context.JobCards.FindAsync(jobCard.JobCardID);

                return CreatedAtAction(nameof(GetJobCard), new { id = createdJobCard.JobCardID }, createdJobCard);
            }

            return BadRequest(ModelState);
        }


        // PUT: api/JobCard/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobCard(Guid id, [FromBody] JobCard jobCard)
        {
            _logger.LogInformation("UpdateJobCard called with ID: {Id}", id);
            _logger.LogInformation("Received JobCard: {@JobCard}", jobCard);

            if (id != jobCard.JobCardID)
            {
                _logger.LogWarning("JobCard ID mismatch: {Id} != {JobCardId}", id, jobCard.JobCardID);
                return BadRequest();
            }

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

            var existingJobCard = await _context.JobCards
                .Include(j => j.PartsUsed)
                .FirstOrDefaultAsync(j => j.JobCardID == id);

            if (existingJobCard == null)
            {
                _logger.LogWarning("JobCard not found with ID: {Id}", id);
                return NotFound();
            }

            existingJobCard.JobNo = jobCard.JobNo;
            existingJobCard.Site = jobCard.Site;
            existingJobCard.Engineer = jobCard.Engineer;
            existingJobCard.RegNo = jobCard.RegNo;
            existingJobCard.CustOrderNo = jobCard.CustOrderNo;
            existingJobCard.Odometer = jobCard.Odometer;
            existingJobCard.Date = jobCard.Date;
            existingJobCard.Time = jobCard.Time;
            existingJobCard.Description = jobCard.Description;
            existingJobCard.SubmissionTime = jobCard.SubmissionTime;
            existingJobCard.RowVersion = jobCard.RowVersion;

            var incomingParts = jobCard.PartsUsed ?? new List<PartUsed>();
            var existingParts = existingJobCard.PartsUsed.ToList();

            var partsToRemove = existingParts.Where(e => !incomingParts.Any(i => i.PartUsedID == e.PartUsedID)).ToList();
            _context.PartsUsed.RemoveRange(partsToRemove);

            foreach (var incomingPart in incomingParts)
            {
                var existingPart = existingParts.FirstOrDefault(e => e.PartUsedID == incomingPart.PartUsedID);
                if (existingPart != null)
                {
                    existingPart.Name = incomingPart.Name;
                    existingPart.PartNumber = incomingPart.PartNumber;
                    existingPart.Quantity = incomingPart.Quantity;
                    existingPart.Category = incomingPart.Category;
                    _context.Entry(existingPart).State = EntityState.Modified;
                }
                else
                {
                    incomingPart.JobCardID = jobCard.JobCardID;
                    _context.PartsUsed.Add(incomingPart);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency exception occurred while updating JobCard with ID: {Id}", id);
                var databaseEntity = await _context.JobCards.AsNoTracking()
                    .FirstOrDefaultAsync(j => j.JobCardID == id);

                if (databaseEntity == null)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.");
                return Conflict(ModelState);
            }

            return NoContent();
        }


        private bool DoesJobCardExist(Guid id)
        {
            return _context.JobCards.Any(e => e.JobCardID == id);
        }










        // DELETE: api/JobCard/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobCard(Guid id)
        {
            var jobCard = await _context.JobCards
                .Include(j => j.PartsUsed)
                .FirstOrDefaultAsync(j => j.JobCardID == id);

            if (jobCard == null)
            {
                return NotFound();
            }

            _context.PartsUsed.RemoveRange(jobCard.PartsUsed);
            _context.JobCards.Remove(jobCard);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool JobCardExists(Guid id)
        {
            return _context.JobCards.Any(e => e.JobCardID == id);
        }
    }
}
