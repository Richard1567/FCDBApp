using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Forms;
using iText.Forms.Fields;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FCDBApp.Models;
using Microsoft.Extensions.Logging;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Image;
using iText.Layout;

namespace FCDBApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobCardExportController : ControllerBase
    {
        private readonly InspectionContext _context;
        private readonly ILogger<JobCardExportController> _logger;

        public JobCardExportController(InspectionContext context, ILogger<JobCardExportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("ExportToPdf/{id}")]
        public async Task<IActionResult> ExportToPdf(Guid id)
        {
            var jobCard = await _context.JobCards
                .Include(j => j.PartsUsed)
                .FirstOrDefaultAsync(m => m.JobCardID == id);

            if (jobCard == null)
            {
                _logger.LogWarning("JobCard with ID {Id} not found.", id);
                return NotFound();
            }

            var template = await _context.TemplateFiles.FirstOrDefaultAsync(t => t.TemplateName == "jobcard_template");
            if (template == null)
            {
                _logger.LogWarning("JobCard template not found.");
                return NotFound("JobCard template not found.");
            }

            var engineerSignature = jobCard.EngineerSignatureID.HasValue
                ? await _context.Signatures.FirstOrDefaultAsync(s => s.SignatureID == jobCard.EngineerSignatureID)
                : null;

            var branchManagerSignature = jobCard.BranchManagerSignatureID.HasValue
                ? await _context.Signatures.FirstOrDefaultAsync(s => s.SignatureID == jobCard.BranchManagerSignatureID)
                : null;

            using (var memoryStream = new MemoryStream())
            {
                var pdfReader = new PdfReader(new MemoryStream(template.TemplateData));
                var pdfWriter = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                var form = PdfAcroForm.GetAcroForm(pdfDocument, true);

                var fields = form.GetAllFormFields();

                SetField(fields, "site", jobCard.Site);
                SetField(fields, "reg", jobCard.RegNo);
                SetField(fields, "odometer", jobCard.Odometer.ToString());
                SetField(fields, "date", jobCard.Date.ToString("dd/MM/yyyy"));
                SetField(fields, "engineer", jobCard.Engineer);
                SetField(fields, "customerno", jobCard.CustOrderNo);
                SetField(fields, "jobno", jobCard.JobNo);
                SetField(fields, "time", jobCard.Time);

                var document = new iText.Layout.Document(pdfDocument);

                if (engineerSignature != null)
                {
                    AddSignatureImage(document, fields, "engineersign", engineerSignature.SignatureImage);
                    SetField(fields, "engineerdate", $"{engineerSignature.Print} {DateTime.Now:dd/MM/yyyy}");
                }

                if (branchManagerSignature != null)
                {
                    AddSignatureImage(document, fields, "branchsign", branchManagerSignature.SignatureImage);
                    SetField(fields, "branchdate", $"{branchManagerSignature.Print} {DateTime.Now:dd/MM/yyyy}");
                }

                // Calculate the y-position for the description
                float descriptionY = 550; // Adjusted y-position based on your template
                document.Add(new Paragraph(jobCard.Description)
                    .SetFontSize(12)
                    .SetFixedPosition(50, descriptionY, 500)); // Adjust x, y, and width as needed

                // Calculate the y-position for the parts used table
                float partsTableY = descriptionY - 100; // Adjust as needed based on your layout

                // Add Parts Used Table
                var partsTable = new iText.Layout.Element.Table(new float[] { 4, 4, 2, 2 });
                partsTable.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));

                partsTable.AddHeaderCell("Part Name");
                partsTable.AddHeaderCell("Part Number");
                partsTable.AddHeaderCell("Quantity");
                partsTable.AddHeaderCell("Category");

                foreach (var part in jobCard.PartsUsed)
                {
                    partsTable.AddCell(part.Name);
                    partsTable.AddCell(part.PartNumber);
                    partsTable.AddCell(part.Quantity.ToString());
                    partsTable.AddCell(part.Category);
                }

                document.Add(partsTable.SetFixedPosition(50, partsTableY, 500)); // Adjust x, y, and width as needed

                form.FlattenFields();
                pdfDocument.Close();

                _logger.LogInformation("JobCard {Id} exported successfully.", id);
                return File(memoryStream.ToArray(), "application/pdf", $"JobCard_{jobCard.JobNo}.pdf");
            }
        }

        private void SetField(IDictionary<string, PdfFormField> fields, string fieldName, string value)
        {
            if (fields.ContainsKey(fieldName))
            {
                fields[fieldName].SetValue(value);
            }
            else
            {
                _logger.LogWarning("Field {FieldName} not found in PDF template.", fieldName);
            }
        }

        private void AddSignatureImage(iText.Layout.Document document, IDictionary<string, PdfFormField> fields, string fieldName, byte[] signatureImage)
        {
            _logger.LogInformation($"Setting signature for field '{fieldName}'");

            if (signatureImage == null || signatureImage.Length == 0)
            {
                _logger.LogWarning($"Signature image for field '{fieldName}' is null or empty");
                return;
            }

            var pdfDoc = document.GetPdfDocument();
            var form = PdfAcroForm.GetAcroForm(pdfDoc, false);

            if (fields.TryGetValue(fieldName, out var signatureField))
            {
                var signatureWidget = signatureField.GetWidgets()[0];
                var signatureRect = signatureWidget.GetRectangle().ToRectangle();

                // Remove the form field
                form.RemoveField(fieldName);

                var signatureImageData = ImageDataFactory.Create(signatureImage);
                var signatureImageObject = new Image(signatureImageData)
                    .SetFixedPosition(signatureRect.GetX(), signatureRect.GetY(), signatureRect.GetWidth())
                    .ScaleAbsolute(signatureRect.GetWidth(), signatureRect.GetHeight());
                document.Add(signatureImageObject);

                _logger.LogInformation($"Added signature image for field '{fieldName}'");
            }
            else
            {
                _logger.LogWarning($"Field '{fieldName}' not found in the form.");
            }
        }
    }
}
