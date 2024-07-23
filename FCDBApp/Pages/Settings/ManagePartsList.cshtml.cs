using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FCDBApp.Models;

namespace FCDBApi.Pages.Settings
{
    public class ManagePartsListModel : PageModel
    {
        private readonly InspectionContext _context;
        private readonly ILogger<ManagePartsListModel> _logger;

        public ManagePartsListModel(InspectionContext context, ILogger<ManagePartsListModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public PartsList Part { get; set; }

        public List<PartsList> PartsList { get; set; }

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Fetching parts list from database.");
            PartsList = await _context.PartsList.ToListAsync();
            _logger.LogInformation($"Fetched {PartsList.Count} parts.");
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            _logger.LogInformation($"Attempting to delete part with ID: {id}");
            var part = await _context.PartsList.FindAsync(id);

            if (part != null)
            {
                _context.PartsList.Remove(part);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Deleted part with ID: {id}");
            }
            else
            {
                _logger.LogWarning($"Part with ID: {id} not found.");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetEditPartAsync(Guid id)
        {
            _logger.LogInformation($"Fetching part with ID: {id} for editing.");
            var part = await _context.PartsList.FindAsync(id);

            if (part == null)
            {
                _logger.LogWarning($"Part with ID: {id} not found.");
                return NotFound();
            }

            _logger.LogInformation($"Found part with ID: {id}. Returning part data.");
            return new JsonResult(part);
        }

        public async Task<IActionResult> OnPostSavePartAsync()
        {
            if (Part == null)
            {
                _logger.LogError("Part data is null.");
                return BadRequest(new { success = false, message = "Invalid part data." });
            }

            _logger.LogInformation($"Received part data: {Part.Name}, {Part.PartNumber}, {Part.PartsListID}");

            if (Part.PartsListID == Guid.Empty)
            {
                Part.PartsListID = Guid.NewGuid();
                _context.PartsList.Add(Part);
                _logger.LogInformation($"Adding new part with ID: {Part.PartsListID}");
            }
            else
            {
                var existingPart = await _context.PartsList.FindAsync(Part.PartsListID);
                if (existingPart == null)
                {
                    _context.PartsList.Add(Part);
                    _logger.LogInformation($"Adding new part with ID: {Part.PartsListID}");
                }
                else
                {
                    _context.Entry(existingPart).CurrentValues.SetValues(Part);
                    _logger.LogInformation($"Updating part with ID: {Part.PartsListID}");
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Saved part successfully.");
                return RedirectToPage();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!PartsListExists(Part.PartsListID))
                {
                    _logger.LogError($"Part with ID: {Part.PartsListID} does not exist.");
                    return NotFound(new { success = false, message = "Part not found." });
                }
                else
                {
                    _logger.LogError($"Concurrency error occurred while saving the part: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while saving the part: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while saving the part." });
            }
        }

        private bool PartsListExists(Guid id)
        {
            return _context.PartsList.Any(e => e.PartsListID == id);
        }
    }
}
