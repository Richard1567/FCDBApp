using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCDBApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCDBApp.Services
{
    public class InspectionSheetService
    {
        private readonly InspectionContext _context;
        private readonly ILogger<InspectionSheetService> _logger;

        public InspectionSheetService(InspectionContext context, ILogger<InspectionSheetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<InspectionTableDto>> GetAllInspectionSheetsAsync()
        {
            var inspections = await _context.InspectionTables
                                            .Include(i => i.Details)
                                            .ThenInclude(d => d.Item)
                                            .ToListAsync();

            return inspections.Select(i => new InspectionTableDto
            {
                InspectionID = i.InspectionID,
                Branch = i.Branch,
                VehicleReg = i.VehicleReg,
                VehicleType = i.VehicleType,
                Odometer = i.Odometer,
                InspectionDate = i.InspectionDate,
                NextInspectionDue = i.NextInspectionDue,
                SubmissionTime = i.SubmissionTime,
                InspectionTypeID = i.InspectionTypeID,
                SiteID = i.SiteID,
                PassFailStatus = i.PassFailStatus,
                Details = i.Details
                            .Where(d => d.Item.InspectionTypeIndicator.Contains(i.InspectionTypeID.ToString()))
                            .Select(d => new InspectionDetailsDto
                            {
                                InspectionDetailID = d.InspectionDetailID,
                                InspectionID = d.InspectionID,
                                InspectionItemID = d.InspectionItemID,
                                Result = d.Result,
                                Comments = d.Comments
                            }).ToList(),
                EngineerSignatureID = i.EngineerSignatureID,
                BranchManagerSignatureID = i.BranchManagerSignatureID
            }).ToList();
        }

        public async Task<InspectionTable> GetInspectionSheetByIdAsync(Guid id)
        {
            var inspection = await _context.InspectionTables
                                           .Include(i => i.Details)
                                           .ThenInclude(d => d.Item)
                                           .FirstOrDefaultAsync(i => i.InspectionID == id);

            return inspection;
        }

        public async Task<InspectionTableDto> GetInspectionSheetDtoByIdAsync(Guid id)
        {
            var inspection = await _context.InspectionTables
                                           .Include(i => i.Details)
                                           .ThenInclude(d => d.Item)
                                           .FirstOrDefaultAsync(i => i.InspectionID == id);

            if (inspection == null)
            {
                return null;
            }

            // Fetch the signatures separately
            var engineerSignature = await GetSignatureByIdAsync(inspection.EngineerSignatureID);
            var branchManagerSignature = await GetSignatureByIdAsync(inspection.BranchManagerSignatureID);

            return new InspectionTableDto
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

            };
        }

        public async Task CreateInspectionSheetAsync(InspectionTable inspectionTable)
        {
            if (inspectionTable.InspectionTypeID == null)
            {
                throw new ArgumentNullException(nameof(inspectionTable.InspectionTypeID), "InspectionTypeID cannot be null.");
            }

            if (inspectionTable.SubmissionTime == DateTime.MinValue)
            {
                inspectionTable.SubmissionTime = DateTime.Now;
                _logger.LogWarning("SubmissionTime was not set. Setting to current time: {SubmissionTime}", inspectionTable.SubmissionTime);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Insert Engineer Signature
                    if (!string.IsNullOrEmpty(inspectionTable.EngineerSignatureBase64))
                    {
                        var engineerSignatureEntity = new Signature
                        {
                            SignatureID = Guid.NewGuid(),
                            SignatureImage = Convert.FromBase64String(inspectionTable.EngineerSignatureBase64.Split(",")[1]),
                            Print = inspectionTable.EngineerPrint,
                            SignatoryType = "Engineer",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Signatures.Add(engineerSignatureEntity);
                        await _context.SaveChangesAsync();
                        inspectionTable.EngineerSignatureID = engineerSignatureEntity.SignatureID;
                    }

                    // Insert Branch Manager Signature
                    if (!string.IsNullOrEmpty(inspectionTable.BranchManagerSignatureBase64))
                    {
                        var branchManagerSignatureEntity = new Signature
                        {
                            SignatureID = Guid.NewGuid(),
                            SignatureImage = Convert.FromBase64String(inspectionTable.BranchManagerSignatureBase64.Split(",")[1]),
                            Print = inspectionTable.BranchManagerPrint,
                            SignatoryType = "BranchManager",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Signatures.Add(branchManagerSignatureEntity);
                        await _context.SaveChangesAsync();
                        inspectionTable.BranchManagerSignatureID = branchManagerSignatureEntity.SignatureID;
                    }

                    _context.InspectionTables.Add(inspectionTable);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating inspection sheet");
                    throw;
                }
            }
        }


        public async Task UpdateInspectionSheetAsync(InspectionTable inspectionTable)
        {
            try
            {
                var existingInspection = await _context.InspectionTables
                    .Include(i => i.Details)
                    .FirstOrDefaultAsync(i => i.InspectionID == inspectionTable.InspectionID);

                if (existingInspection == null)
                {
                    throw new Exception("Unable to save changes. The inspection sheet was deleted by another user.");
                }

                existingInspection.Branch = inspectionTable.Branch;
                existingInspection.VehicleReg = inspectionTable.VehicleReg;
                existingInspection.VehicleType = inspectionTable.VehicleType;
                existingInspection.InspectionDate = inspectionTable.InspectionDate;
                existingInspection.NextInspectionDue = inspectionTable.NextInspectionDue;
                existingInspection.SubmissionTime = DateTime.Now;
                existingInspection.InspectionTypeID = inspectionTable.InspectionTypeID;
                existingInspection.PassFailStatus = inspectionTable.PassFailStatus;

                var incomingDetails = inspectionTable.Details ?? new List<InspectionDetails>();
                var existingDetails = existingInspection.Details.ToList();

                var detailsToRemove = existingDetails.Where(e => !incomingDetails.Any(i => i.InspectionDetailID == e.InspectionDetailID)).ToList();
                _context.InspectionDetails.RemoveRange(detailsToRemove);

                foreach (var incomingDetail in incomingDetails)
                {
                    var existingDetail = existingDetails.FirstOrDefault(e => e.InspectionDetailID == incomingDetail.InspectionDetailID);
                    if (existingDetail != null)
                    {
                        existingDetail.Result = incomingDetail.Result;
                        existingDetail.Comments = incomingDetail.Comments;
                        _context.Entry(existingDetail).State = EntityState.Modified;
                    }
                    else
                    {
                        var newDetail = new InspectionDetails
                        {
                            InspectionID = inspectionTable.InspectionID,
                            InspectionItemID = incomingDetail.InspectionItemID,
                            Result = incomingDetail.Result,
                            Comments = incomingDetail.Comments
                        };
                        _context.InspectionDetails.Add(newDetail);
                    }
                }

                // Update signatures if provided
                if (inspectionTable.EngineerSignatureID.HasValue)
                {
                    existingInspection.EngineerSignatureID = inspectionTable.EngineerSignatureID;
                }

                if (inspectionTable.BranchManagerSignatureID.HasValue)
                {
                    existingInspection.BranchManagerSignatureID = inspectionTable.BranchManagerSignatureID;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Inspection sheet updated successfully.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("Unable to save changes. The entity was deleted by another user.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the inspection sheet.", ex);
            }
        }

        public async Task DeleteInspectionSheetAsync(Guid id)
        {
            var inspectionSheet = await _context.InspectionTables
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.InspectionID == id);

            if (inspectionSheet != null)
            {
                _context.InspectionDetails.RemoveRange(inspectionSheet.Details);
                _context.InspectionTables.Remove(inspectionSheet);

                // Optionally, remove associated signatures if they are not used elsewhere
                if (inspectionSheet.EngineerSignatureID.HasValue)
                {
                    var engineerSignature = await _context.Signatures.FindAsync(inspectionSheet.EngineerSignatureID);
                    if (engineerSignature != null)
                    {
                        _context.Signatures.Remove(engineerSignature);
                    }
                }

                if (inspectionSheet.BranchManagerSignatureID.HasValue)
                {
                    var branchManagerSignature = await _context.Signatures.FindAsync(inspectionSheet.BranchManagerSignatureID);
                    if (branchManagerSignature != null)
                    {
                        _context.Signatures.Remove(branchManagerSignature);
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<InspectionType>> GetInspectionTypesAsync()
        {
            return await _context.InspectionTypes.ToListAsync();
        }

        public async Task<List<InspectionCategories>> GetInspectionCategoriesWithItemsForTypeAsync(int inspectionTypeId)
        {
            _logger.LogInformation($"Fetching categories for inspection type ID containing: {inspectionTypeId}");

            var inspectionTypeIdString = inspectionTypeId.ToString();
            var categories = await _context.InspectionCategories
                .Include(c => c.Items)
                .Where(c => c.Items.Any(i => EF.Functions.Like(i.InspectionTypeIndicator, "%" + inspectionTypeIdString + "%")))
                .ToListAsync();

            if (categories == null || !categories.Any())
            {
                _logger.LogWarning("No categories found for the provided inspection type ID.");
            }
            else
            {
                _logger.LogInformation($"Fetched {categories.Count} categories for inspection type ID containing: {inspectionTypeId}");
            }

            return categories;
        }

        public async Task<List<InspectionCategoryDto>> GetInspectionCategoriesDtoWithItemsForTypeAsync(int inspectionTypeId)
        {
            _logger.LogInformation($"Fetching categories for inspection type ID containing: {inspectionTypeId}");

            var inspectionTypeIdString = inspectionTypeId.ToString();
            var categories = await _context.InspectionCategories
                .Include(c => c.Items)
                .Where(c => c.Items.Any(i => EF.Functions.Like(i.InspectionTypeIndicator, "%" + inspectionTypeIdString + "%")))
                .Select(c => new InspectionCategoryDto
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    Items = c.Items
                        .Where(i => EF.Functions.Like(i.InspectionTypeIndicator, "%" + inspectionTypeIdString + "%"))
                        .Select(i => new InspectionItemDto
                        {
                            InspectionItemID = i.InspectionItemID,
                            ItemDescription = i.ItemDescription,
                            CategoryID = i.CategoryID,
                            InspectionTypeIndicator = i.InspectionTypeIndicator
                        }).ToList()
                }).ToListAsync();

            if (categories == null || !categories.Any())
            {
                _logger.LogWarning("No categories found for the provided inspection type ID.");
            }
            else
            {
                _logger.LogInformation($"Fetched {categories.Count} categories for inspection type ID containing: {inspectionTypeId}");

                // Log detailed category information
                foreach (var category in categories)
                {
                    _logger.LogInformation($"CategoryID: {category.CategoryID}, CategoryName: {category.CategoryName}");
                    foreach (var item in category.Items)
                    {
                        _logger.LogInformation($"ItemID: {item.InspectionItemID}, ItemDescription: {item.ItemDescription}, InspectionTypeIndicator: {item.InspectionTypeIndicator}");
                    }
                }
            }

            return categories;
        }

        public async Task<InspectionTableDto> GetFilteredInspectionSheetByIdAsync(Guid id, int inspectionTypeId)
        {
            var inspection = await _context.InspectionTables
                                           .Include(i => i.Details)
                                           .ThenInclude(d => d.Item)
                                           .FirstOrDefaultAsync(i => i.InspectionID == id);

            if (inspection == null)
            {
                return null;
            }

            var validItemIds = _context.InspectionItems
                                       .Where(item => item.InspectionTypeIndicator.Contains(inspectionTypeId.ToString()))
                                       .Select(item => item.InspectionItemID)
                                       .ToList();

            var filteredDetails = inspection.Details.Where(d => validItemIds.Contains(d.InspectionItemID)).ToList();

            return new InspectionTableDto
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
                Details = filteredDetails.Select(d => new InspectionDetailsDto
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList(),
                EngineerSignatureID = inspection.EngineerSignatureID,
                BranchManagerSignatureID = inspection.BranchManagerSignatureID
            };
        }

        public async Task<Signature> GetSignatureByIdAsync(Guid? signatureId)
        {
            if (signatureId == null)
            {
                return null;
            }

            return await _context.Signatures.FindAsync(signatureId);
        }

        public async Task<List<Signature>> GetSignaturesByIdsAsync(params Guid?[] signatureIds)
        {
            return await _context.Signatures
                .Where(s => signatureIds.Contains(s.SignatureID))
                .ToListAsync();
        }
    }
}
