using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FCDBApi.Pages.Settings
{
    public class ManageTemplatesModel : PageModel
    {
        private readonly InspectionContext _context;
        private readonly ILogger<ManageTemplatesModel> _logger;

        public ManageTemplatesModel(InspectionContext context, ILogger<ManageTemplatesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public List<TemplateFile> TemplateFiles { get; set; }

        [BindProperty]
        public TemplateFile NewTemplate { get; set; }

        public async Task OnGetAsync()
        {
            TemplateFiles = await _context.TemplateFiles.ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (NewTemplate == null || Request.Form.Files.Count == 0)
            {
                ModelState.AddModelError("", "Invalid template data");
                return Page();
            }

            var formFile = Request.Form.Files[0];
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);
                NewTemplate.TemplateData = memoryStream.ToArray();
                NewTemplate.ContentType = formFile.ContentType;
            }

            _context.TemplateFiles.Add(NewTemplate);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int templateId, string templateName)
        {
            var template = await _context.TemplateFiles.FindAsync(templateId);
            if (template == null)
            {
                return NotFound();
            }

            template.TemplateName = templateName;
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int templateId)
        {
            var template = await _context.TemplateFiles.FindAsync(templateId);
            if (template == null)
            {
                return NotFound();
            }

            _context.TemplateFiles.Remove(template);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
