﻿using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using iText.IO.Image;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class ExportHandler
{
    private readonly InspectionContext _context;
    private readonly ILogger<ExportHandler> _logger;
    private readonly string _outputPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Outputs");

    public ExportHandler(InspectionContext context, ILogger<ExportHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateInspectionReportAsync(Guid inspectionId)
    {
        try
        {
            // Fetch the template from the database
            var template = _context.TemplateFiles.FirstOrDefault(t => t.TemplateName == "inspection_template");
            if (template == null)
            {
                _logger.LogError("Template not found in database.");
                throw new FileNotFoundException("Template not found in database.");
            }

            var inspection = await _context.InspectionTables
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.InspectionID == inspectionId);

            if (inspection == null)
            {
                _logger.LogWarning($"Inspection with ID {inspectionId} not found.");
                return null;
            }

            _logger.LogInformation($"Retrieved Inspection: ID={inspection.InspectionID}, EngineerSignatureID={inspection.EngineerSignatureID}, BranchManagerSignatureID={inspection.BranchManagerSignatureID}");

            var engineerSignature = await FetchSignatureAsync(inspection.EngineerSignatureID);
            var branchManagerSignature = await FetchSignatureAsync(inspection.BranchManagerSignatureID);

            if (engineerSignature != null)
            {
                _logger.LogInformation($"Fetched Engineer Signature: ID={engineerSignature.SignatureID}, Print={engineerSignature.Print}");
            }
            else
            {
                _logger.LogWarning("Engineer signature not found.");
            }

            if (branchManagerSignature != null)
            {
                _logger.LogInformation($"Fetched Branch Manager Signature: ID={branchManagerSignature.SignatureID}, Print={branchManagerSignature.Print}");
            }
            else
            {
                _logger.LogWarning("Branch manager signature not found.");
            }

            var data = new InspectionTableDto
            {
                InspectionID = inspection.InspectionID,
                Branch = inspection.Branch,
                VehicleReg = inspection.VehicleReg,
                VehicleType = inspection.VehicleType,
                Odometer = inspection.Odometer,
                InspectionDate = inspection.InspectionDate,
                NextInspectionDue = inspection.NextInspectionDue,
                SubmissionTime = inspection.SubmissionTime,
                InspectionTypeID = inspection.InspectionTypeID,
                SiteID = inspection.SiteID,
                PassFailStatus = inspection.PassFailStatus,
                Details = inspection.Details.Select(d => new InspectionDetailsDto
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList(),
                EngineerSignatureID = inspection.EngineerSignatureID,
                EngineerSignature = engineerSignature,
                BranchManagerSignatureID = inspection.BranchManagerSignatureID,
                BranchManagerSignature = branchManagerSignature
            };

            var filledTemplatePath = System.IO.Path.Combine(_outputPath, $"inspection_filled_{data.InspectionID}.pdf");

            // Load the PDF template from the database
            using (var pdfReader = new PdfReader(new MemoryStream(template.TemplateData)))
            using (var pdfWriter = new PdfWriter(filledTemplatePath))
            using (var pdfDoc = new PdfDocument(pdfReader, pdfWriter))
            {
                var form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                var fields = form.GetAllFormFields();

                // Create a document object for adding text and graphics
                var document = new iText.Layout.Document(pdfDoc);

                FillTemplate(document, form, fields, data);

                // Flatten fields if any remain
                form.FlattenFields();
            }

            _logger.LogInformation($"PDF report generated at {filledTemplatePath}");
            return filledTemplatePath;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to generate inspection report: {ex.Message}");
            throw;
        }
    }

    private async Task<SignatureDto> FetchSignatureAsync(Guid? signatureId)
    {
        if (signatureId == null)
        {
            _logger.LogWarning("Signature ID is null.");
            return null;
        }

        var signature = await _context.Signatures
            .Where(s => s.SignatureID == signatureId)
            .Select(s => new SignatureDto
            {
                SignatureID = s.SignatureID,
                Print = s.Print,
                SignatureImage = s.SignatureImage,
                SignatoryType = s.SignatoryType
            })
            .FirstOrDefaultAsync();

        if (signature == null)
        {
            _logger.LogWarning($"No signature found for SignatureID: {signatureId}");
        }

        return signature;
    }

    private void FillTemplate(iText.Layout.Document document, PdfAcroForm form, IDictionary<string, PdfFormField> fields, InspectionTableDto data)
    {
        _logger.LogInformation($"Populating fields for inspection ID: {data.InspectionID}");
        _logger.LogInformation($"Pass/Fail Status: {data.PassFailStatus}");

        // Log the signature IDs
        _logger.LogInformation($"EngineerSigID: {data.EngineerSignatureID}");
        _logger.LogInformation($"BranchSigID: {data.BranchManagerSignatureID}");

        // Replace these fields
        ReplaceTextBoxWithText(document, form, fields, "branch", data.Branch, 12);
        ReplaceTextBoxWithText(document, form, fields, "reg", data.VehicleReg, 12);
        ReplaceTextBoxWithText(document, form, fields, "type", data.VehicleType, 12);
        ReplaceTextBoxWithText(document, form, fields, "odometer", data.Odometer.ToString(), 12);
        ReplaceTextBoxWithText(document, form, fields, "date", data.InspectionDate.ToString("dd/MM/yyyy"), 12);
        ReplaceTextBoxWithText(document, form, fields, "nextdate", data.NextInspectionDue.ToString("dd/MM/yyyy"), 12);
        ReplaceTextBoxWithText(document, form, fields, "notes", CombineNotes(data.Details), 8);

        // Draw the pass/fail status rectangle
        DrawPassFailRectangle(document, data.PassFailStatus);

        // Populate these fields
        SetFormField(fields, "engineerprint", data.EngineerSignature?.Print);
        AddSignatureImage(document, fields, "engineersign", data.EngineerSignature?.SignatureImage);

        // Populate these fields
        SetFormField(fields, "branchprint", data.BranchManagerSignature?.Print);
        AddSignatureImage(document, fields, "branchsign", data.BranchManagerSignature?.SignatureImage);

        // Determine which set of fields to fill based on InspectionTypeID
        string prefix;
        switch (data.InspectionTypeID)
        {
            case 1:
                prefix = "a";
                break;
            case 2:
                prefix = "b";
                break;
            case 3:
                prefix = "c";
                break;
            case 4:
                prefix = "d";
                break;
            default:
                throw new ArgumentException("Invalid Inspection Type ID");
        }

        _logger.LogInformation($"Using prefix: {prefix} for InspectionTypeID: {data.InspectionTypeID}");

        // Replace the inspection results
        for (int i = 1; i <= 69; i++)
        {
            string fieldName = $"{prefix}{i}";
            if (fields.ContainsKey(fieldName))
            {
                var result = data.Details.ElementAtOrDefault(i - 1)?.Result ?? "";
                var displayResult = result.ToLower() == "y" ? "Pass" : result.ToLower() == "n" ? "Fail" : result;
                _logger.LogInformation($"Field {fieldName}: {displayResult}");
                ReplaceTextBoxWithText(document, form, fields, fieldName, displayResult, 8); // Use a smaller font size for inspection results
            }
            else
            {
                _logger.LogWarning($"Field {fieldName} not found in the form.");
            }
        }
    }

    private void SetFormField(IDictionary<string, PdfFormField> fields, string fieldName, string value)
    {
        _logger.LogInformation($"Setting field '{fieldName}' with value: {value}");

        if (fields.ContainsKey(fieldName))
        {
            fields[fieldName].SetValue(value);
        }
        else
        {
            _logger.LogWarning($"Field '{fieldName}' not found in the form.");
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

    private void DrawPassFailRectangle(iText.Layout.Document document, string passFailStatus)
    {
        if (document == null || string.IsNullOrEmpty(passFailStatus))
        {
            _logger.LogError("Document or passFailStatus is null.");
            return;
        }

        float x, y, width = 28.35f, height = 14.17f; // 1cm = 28.35 points, 0.5cm = 14.17 points

        // LibreOffice origin is top-left, so we need to adjust the y-coordinate
        float pageHeight = document.GetPdfDocument().GetFirstPage().GetPageSize().GetHeight();

        if (passFailStatus.Equals("Fail", StringComparison.OrdinalIgnoreCase))
        {
            x = 12.96f * 28.35f;
            y = pageHeight - (20.60f * 28.35f + height); // Adjust y-coordinate
        }
        else if (passFailStatus.Equals("Pass", StringComparison.OrdinalIgnoreCase))
        {
            x = 14.00f * 28.35f;
            y = pageHeight - (20.60f * 28.35f + height); // Adjust y-coordinate
        }
        else
        {
            _logger.LogError("Invalid Pass/Fail status.");
            return;
        }

        var rectangle = new Rectangle(x, y, width, height);
        var canvas = new PdfCanvas(document.GetPdfDocument().GetFirstPage());
        canvas.SetFillColor(ColorConstants.BLACK)
              .Rectangle(rectangle)
              .Fill();
    }

    private string CombineNotes(ICollection<InspectionDetailsDto> details)
    {
        return string.Join("; ", details.Where(d => !string.IsNullOrWhiteSpace(d.Comments)).Select(d => d.Comments));
    }

    [HttpPost("capture-signature")]
    public async Task<IActionResult> CaptureSignature(
        [FromQuery] Guid[] inspectionIds,
        [FromQuery] Guid[] jobCardIds,
        [FromBody] byte[] signatureImage,
        [FromQuery] string signatoryType)
    {
        if (signatureImage == null || string.IsNullOrEmpty(signatoryType))
        {
            return new JsonResult(new { Message = "Invalid signature data." }) { StatusCode = 400 };
        }

        var signature = new Signature
        {
            SignatureID = Guid.NewGuid(),
            SignatureImage = signatureImage,
            SignatoryType = signatoryType,
            CreatedAt = DateTime.UtcNow
        };

        _context.Signatures.Add(signature);

        if (inspectionIds != null && inspectionIds.Length > 0)
        {
            foreach (var inspectionId in inspectionIds)
            {
                var inspection = await _context.InspectionTables.FindAsync(inspectionId);
                if (inspection != null)
                {
                    if (signatoryType == "Engineer")
                    {
                        inspection.EngineerSignatureID = signature.SignatureID;
                    }
                    else if (signatoryType == "BranchManager")
                    {
                        inspection.BranchManagerSignatureID = signature.SignatureID;
                    }
                }
            }
        }

        if (jobCardIds != null && jobCardIds.Length > 0)
        {
            foreach (var jobCardId in jobCardIds)
            {
                var jobCard = await _context.JobCards.FindAsync(jobCardId);
                if (jobCard != null)
                {
                    if (signatoryType == "Engineer")
                    {
                        jobCard.EngineerSignatureID = signature.SignatureID;
                    }
                    else if (signatoryType == "BranchManager")
                    {
                        jobCard.BranchManagerSignatureID = signature.SignatureID;
                    }
                }
            }
        }


        await _context.SaveChangesAsync();

        return new JsonResult(new { Message = "Signature captured and associated successfully." }) { StatusCode = 200 };
    }
    private void ReplaceTextBoxWithText(iText.Layout.Document document, PdfAcroForm form, IDictionary<string, PdfFormField> fields, string fieldName, string value, float fontSize)
    {
        if (fields.TryGetValue(fieldName, out var field))
        {
            var widget = field.GetWidgets()[0];
            var rect = widget.GetRectangle().ToRectangle();

            _logger.LogInformation($"Replacing text box field '{fieldName}' with value: {value}");

            // Remove the form field
            form.RemoveField(fieldName);

            if (!string.IsNullOrEmpty(value))
            {
                // Add the text at the position of the form field
                var paragraph = new Paragraph(value)
                    .SetFixedPosition(rect.GetX(), rect.GetY(), rect.GetWidth())
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE) // Center vertically
                    .SetFontSize(fontSize); // Use the specified font size
                document.Add(paragraph);

                _logger.LogInformation($"Replaced text box field '{fieldName}' with value: {value}");
            }
        }
        else
        {
            _logger.LogWarning($"Field '{fieldName}' not found in the form.");
        }
    }
}
