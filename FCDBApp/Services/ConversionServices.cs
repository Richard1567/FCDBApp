using FCDBApp.Models;
using System;
using System.Linq;

namespace FCDBApp.Services
{
    public static class ConversionService
    {
        public static InspectionTable ToModel(this InspectionTableDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "DTO cannot be null.");
            }

            try
            {
                return new InspectionTable
                {
                    InspectionID = dto.InspectionID,
                    Branch = dto.Branch,
                    VehicleReg = dto.VehicleReg,
                    VehicleType = dto.VehicleType,
                    InspectionDate = dto.InspectionDate,
                    NextInspectionDue = dto.NextInspectionDue,
                    SubmissionTime = dto.SubmissionTime,
                    InspectionTypeID = dto.InspectionTypeID,
                    Details = dto.Details?.Select(d => new InspectionDetails
                    {
                        InspectionDetailID = d.InspectionDetailID,
                        InspectionID = d.InspectionID,
                        InspectionItemID = d.InspectionItemID,
                        Result = d.Result,
                        Comments = d.Comments
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while converting DTO to model.", ex);
            }
        }

        public static InspectionTableDto ToDto(this InspectionTable model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Model cannot be null.");
            }

            try
            {
                return new InspectionTableDto
                {
                    InspectionID = model.InspectionID,
                    Branch = model.Branch,
                    VehicleReg = model.VehicleReg,
                    VehicleType = model.VehicleType,
                    InspectionDate = model.InspectionDate,
                    NextInspectionDue = model.NextInspectionDue,
                    SubmissionTime = model.SubmissionTime,
                    InspectionTypeID = model.InspectionTypeID,
                    Details = model.Details?.Select(d => new InspectionDetailsDto
                    {
                        InspectionDetailID = d.InspectionDetailID,
                        InspectionID = d.InspectionID,
                        InspectionItemID = d.InspectionItemID,
                        Result = d.Result,
                        Comments = d.Comments
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while converting model to DTO.", ex);
            }
        }
    }
}
