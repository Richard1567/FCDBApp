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
                InspectionDate = i.InspectionDate,
                NextInspectionDue = i.NextInspectionDue,
                SubmissionTime = i.SubmissionTime,
                InspectionTypeID = i.InspectionTypeID,
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
                            }).ToList()
            }).ToList();
        }


        public async Task<InspectionTable> GetInspectionSheetByIdAsync(Guid id)
        {
            var inspection = await _context.InspectionTables
                                           .Include(i => i.Details)
                                           .ThenInclude(d => d.Item)
                                           .FirstOrDefaultAsync(i => i.InspectionID == id);

            if (inspection == null)
            {
                return null;
            }

            return new InspectionTable
            {
                InspectionID = inspection.InspectionID,
                Branch = inspection.Branch,
                VehicleReg = inspection.VehicleReg,
                VehicleType = inspection.VehicleType,
                InspectionDate = inspection.InspectionDate,
                NextInspectionDue = inspection.NextInspectionDue,
                SubmissionTime = inspection.SubmissionTime,
                InspectionTypeID = inspection.InspectionTypeID,
                PassFailStatus = inspection.PassFailStatus, 
                Details = inspection.Details.Select(d => new InspectionDetails
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments,
                    Item = new InspectionItems
                    {
                        InspectionItemID = d.Item.InspectionItemID,
                        CategoryID = d.Item.CategoryID,
                        ItemDescription = d.Item.ItemDescription,
                        InspectionTypeIndicator = d.Item.InspectionTypeIndicator,
                        InspectionTypeID = d.Item.InspectionTypeID
                    }
                }).ToList()
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

            _logger.LogInformation("Creating inspection sheet with type ID: {InspectionTypeID}", inspectionTable.InspectionTypeID);
            _logger.LogInformation("SubmissionTime: {SubmissionTime}", inspectionTable.SubmissionTime);

            try
            {
                _context.InspectionTables.Add(inspectionTable);

                foreach (var detail in inspectionTable.Details)
                {
                    _logger.LogInformation("Adding detail to context: {Detail}", detail);
                    _context.InspectionDetails.Add(detail);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Inspection sheet created successfully with ID: {InspectionID}", inspectionTable.InspectionID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inspection sheet");
                throw;
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

                await _context.SaveChangesAsync();
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

            return new InspectionTableDto
            {
                InspectionID = inspection.InspectionID,
                Branch = inspection.Branch,
                VehicleReg = inspection.VehicleReg,
                VehicleType = inspection.VehicleType,
                InspectionDate = inspection.InspectionDate,
                NextInspectionDue = inspection.NextInspectionDue,
                SubmissionTime = inspection.SubmissionTime,
                InspectionTypeID = inspection.InspectionTypeID,
                PassFailStatus = inspection.PassFailStatus,
                Details = inspection.Details.Select(d => new InspectionDetailsDto
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList()
            };
        }


        public async Task UpdateInspectionSheetDtoAsync(InspectionTableDto inspectionDto)
        {
            try
            {
                var existingInspection = await _context.InspectionTables
                    .Include(i => i.Details)
                    .FirstOrDefaultAsync(i => i.InspectionID == inspectionDto.InspectionID);

                if (existingInspection == null)
                {
                    throw new Exception("Unable to save changes. The inspection sheet was deleted by another user.");
                }

                existingInspection.Branch = inspectionDto.Branch;
                existingInspection.VehicleReg = inspectionDto.VehicleReg;
                existingInspection.VehicleType = inspectionDto.VehicleType;
                existingInspection.InspectionDate = inspectionDto.InspectionDate;
                existingInspection.NextInspectionDue = inspectionDto.NextInspectionDue;
                existingInspection.SubmissionTime = DateTime.Now;
                existingInspection.InspectionTypeID = inspectionDto.InspectionTypeID;
                existingInspection.PassFailStatus = inspectionDto.PassFailStatus;   
                var incomingDetails = inspectionDto.Details ?? new List<InspectionDetailsDto>();
                var existingDetails = existingInspection.Details.ToList();

                _logger.LogInformation($"Existing details count: {existingDetails.Count}");
                _logger.LogInformation($"Incoming details count: {incomingDetails.Count}");

                var detailsToRemove = existingDetails.Where(e => !incomingDetails.Any(i => i.InspectionDetailID == e.InspectionDetailID)).ToList();
                _logger.LogInformation($"Details to remove count: {detailsToRemove.Count}");
                _context.InspectionDetails.RemoveRange(detailsToRemove);

                foreach (var incomingDetail in incomingDetails)
                {
                    var existingDetail = existingDetails.FirstOrDefault(e => e.InspectionDetailID == incomingDetail.InspectionDetailID);
                    if (existingDetail != null)
                    {
                        existingDetail.Result = incomingDetail.Result;
                        existingDetail.Comments = incomingDetail.Comments;
                        _context.Entry(existingDetail).State = EntityState.Modified;
                        _logger.LogInformation($"Updated detail: DetailID={existingDetail.InspectionDetailID}, Result={existingDetail.Result}, Comments={existingDetail.Comments}");
                    }
                    else
                    {
                        var newDetail = new InspectionDetails
                        {
                            InspectionID = inspectionDto.InspectionID,
                            InspectionItemID = incomingDetail.InspectionItemID,
                            Result = incomingDetail.Result,
                            Comments = incomingDetail.Comments
                        };
                        _context.InspectionDetails.Add(newDetail);
                        _logger.LogInformation($"Added new detail: ItemID={newDetail.InspectionItemID}, Result={newDetail.Result}, Comments={newDetail.Comments}");
                    }
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

            var inspectionDto = new InspectionTableDto
            {
                InspectionID = inspection.InspectionID,
                Branch = inspection.Branch,
                VehicleReg = inspection.VehicleReg,
                VehicleType = inspection.VehicleType,
                InspectionDate = inspection.InspectionDate,
                NextInspectionDue = inspection.NextInspectionDue,
                SubmissionTime = inspection.SubmissionTime,
                InspectionTypeID = inspection.InspectionTypeID,
                PassFailStatus = inspection.PassFailStatus, 
                Details = filteredDetails.Select(d => new InspectionDetailsDto
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList()
            };

            return inspectionDto;
        }




    }
}
