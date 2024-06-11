using FCDBApp.Models;
using System.Linq;

namespace FCDBApp.Services
{
    public static class ConversionService
    {
        public static InspectionTable ToModel(this InspectionTableDto dto)
        {
            if (dto == null) return null;

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
                Details = dto.Details.Select(d => new InspectionDetails
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList()
            };
        }

        public static InspectionTableDto ToDto(this InspectionTable model)
        {
            if (model == null) return null;

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
                Details = model.Details.Select(d => new InspectionDetailsDto
                {
                    InspectionDetailID = d.InspectionDetailID,
                    InspectionID = d.InspectionID,
                    InspectionItemID = d.InspectionItemID,
                    Result = d.Result,
                    Comments = d.Comments
                }).ToList()
            };
        }
    }
}
