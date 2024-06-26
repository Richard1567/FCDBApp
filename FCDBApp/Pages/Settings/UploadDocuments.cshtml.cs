using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FCDBApp.Models;

namespace FCDBApi.Pages.Settings
{
    public class UploadDocumentsModel : PageModel
    {
        private readonly InspectionContext _context;
        private readonly ILogger<UploadDocumentsModel> _logger;

        public UploadDocumentsModel(InspectionContext context, ILogger<UploadDocumentsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public string DocumentName { get; set; }

        [BindProperty]
        public string? Notes { get; set; }

        [BindProperty]
        public string SelectedCategory { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public List<DocumentCategory> DocumentCategories { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            DocumentCategories = await _context.DocumentCategories.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogWarning($"Model state error: {error.ErrorMessage}");
                        }
                    }
                    DocumentCategories = await _context.DocumentCategories.ToListAsync();
                    return Page();
                }

                int categoryId;
                if (SelectedCategory == "AddNew")
                {
                    string newCategoryName = Request.Form["newCategoryName"];
                    var newCategory = new DocumentCategory { CategoryName = newCategoryName };
                    _context.DocumentCategories.Add(newCategory);
                    await _context.SaveChangesAsync();
                    categoryId = newCategory.DocumentCategoryID;
                    _logger.LogInformation($"New category created with ID: {categoryId}");
                }
                else
                {
                    categoryId = int.Parse(SelectedCategory);
                    _logger.LogInformation($"Existing category selected with ID: {categoryId}");
                }

                if (Upload != null && Upload.Length > 0)
                {
                    var document = new Document
                    {
                        DocumentName = DocumentName,
                        Notes = Notes,
                        DocumentCategoryID = categoryId,
                        UploadDate = DateTime.UtcNow
                    };

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, Upload.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Upload.CopyToAsync(stream);
                    }

                    document.DocumentPath = "/uploads/" + Upload.FileName;

                    _context.Documents.Add(document);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Document uploaded and saved successfully.");
                }
                else
                {
                    _logger.LogWarning("No file selected for upload.");
                    ModelState.AddModelError("Upload", "Please select a file to upload.");
                    DocumentCategories = await _context.DocumentCategories.ToListAsync();
                    return Page();
                }

                return RedirectToPage("./ManageDocuments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading the document.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while uploading the document. Please try again.");
                DocumentCategories = await _context.DocumentCategories.ToListAsync();
                return Page();
            }
        }

    }
}
