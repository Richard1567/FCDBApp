using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FCDBApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FCDBApi.Pages.Settings
{
    public class UploadDocumentsModel : PageModel
    {
        private readonly InspectionContext _context;
        private readonly ILogger<UploadDocumentsModel> _logger;

        public UploadDocumentsModel(InspectionContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string DocumentName { get; set; }

        [BindProperty]
        public string? Notes { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        [BindProperty]
        public string SelectedCategory { get; set; }

        public List<DocumentCategory> DocumentCategories { get; set; }

        public async Task OnGetAsync()
        {
            DocumentCategories = await _context.DocumentCategories.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
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
                return Page();
            }

            string newCategoryName = Request.Form["newCategoryName"];
            int documentCategoryId;

            if (!string.IsNullOrWhiteSpace(newCategoryName))
            {
                var newCategory = new DocumentCategory { CategoryName = newCategoryName };
                _context.DocumentCategories.Add(newCategory);
                await _context.SaveChangesAsync();
                documentCategoryId = newCategory.DocumentCategoryID;
            }
            else
            {
                documentCategoryId = int.Parse(Request.Form["SelectedCategory"]);
            }

            if (Upload != null && Upload.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await Upload.CopyToAsync(memoryStream);
                var document = new Document
                {
                    DocumentName = DocumentName,
                    Notes = Notes, // Allow null
                    DocumentCategoryID = documentCategoryId,
                    DocumentData = memoryStream.ToArray(),
                    ContentType = Upload.ContentType,
                    UploadDate = DateTime.UtcNow // Ensure a valid datetime value
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();
                return RedirectToPage("./ManageDocuments");
            }

            ModelState.AddModelError(string.Empty, "Upload failed, please try again.");
            DocumentCategories = await _context.DocumentCategories.ToListAsync();
            return Page();
        }

    }
}
