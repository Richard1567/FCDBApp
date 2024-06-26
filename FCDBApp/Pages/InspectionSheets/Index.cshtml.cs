using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FCDBApp.Models;

namespace FCDBApi.Pages.InspectionSheets
{
    public class IndexModel : PageModel
    {
        private readonly FCDBApp.Models.InspectionContext _context;

        public IndexModel(FCDBApp.Models.InspectionContext context)
        {
            _context = context;
        }

        public IList<InspectionTable> InspectionTable { get; set; } = default!;
        public Dictionary<int, string> SiteNames { get; set; }

        public async Task OnGetAsync()
        {
            InspectionTable = await _context.InspectionTables.ToListAsync();
            SiteNames = await _context.Sites.ToDictionaryAsync(s => s.SiteID, s => s.SiteName);
        }
    }
}
