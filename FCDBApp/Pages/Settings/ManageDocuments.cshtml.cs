using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FCDBApi.Pages.Settings
{
    public class ManageDocumentsModel : PageModel
    {
        private readonly InspectionContext _context;

        public ManageDocumentsModel(InspectionContext context)
        {
            _context = context;
        }

        public IList<DocumentCategory> DocumentCategories { get; set; }

        public async Task OnGetAsync()
        {
            DocumentCategories = await _context.DocumentCategories.Include(c => c.Documents).ToListAsync();
        }

        public async Task<IActionResult> OnPostEditDocumentAsync(int DocumentID, string DocumentName, string Notes, int DocumentCategoryID)
        {
            var document = await _context.Documents.FindAsync(DocumentID);
            if (document == null)
            {
                return NotFound();
            }

            document.DocumentName = DocumentName;
            document.Notes = Notes;
            document.DocumentCategoryID = DocumentCategoryID;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteDocumentAsync(int DocumentID)
        {
            var document = await _context.Documents.FindAsync(DocumentID);
            if (document == null)
            {
                return NotFound();
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditCategoryAsync(int DocumentCategoryID, string CategoryName)
        {
            var category = await _context.DocumentCategories.FindAsync(DocumentCategoryID);
            if (category == null)
            {
                return NotFound();
            }

            category.CategoryName = CategoryName;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(int DocumentCategoryID)
        {
            var category = await _context.DocumentCategories.FindAsync(DocumentCategoryID);
            if (category == null)
            {
                return NotFound();
            }

            _context.DocumentCategories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddCategoryAsync(string CategoryName)
        {
            var newCategory = new DocumentCategory { CategoryName = CategoryName };
            _context.DocumentCategories.Add(newCategory);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
