using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FCDBApi.Pages.Settings
{
    public class ManageSitesModel : PageModel
    {
        private readonly InspectionContext _context;

        public ManageSitesModel(InspectionContext context)
        {
            _context = context;
        }

        public List<Site> Sites { get; set; }

        [BindProperty]
        public string NewSiteName { get; set; }

        public async Task OnGetAsync()
        {
            Sites = await _context.Sites.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(NewSiteName))
            {
                var newSite = new Site { SiteName = NewSiteName };
                _context.Sites.Add(newSite);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
