using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FCDBApi.Pages.Settings
{
    public class EditSiteModel : PageModel
    {
        private readonly InspectionContext _context;
        private readonly ILogger<EditSiteModel> _logger;

        public EditSiteModel(InspectionContext context, ILogger<EditSiteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Site Site { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Site = await _context.Sites.FindAsync(id);
            if (Site == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Site).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RedirectToPage("ManageSites");
        }
    }
}
