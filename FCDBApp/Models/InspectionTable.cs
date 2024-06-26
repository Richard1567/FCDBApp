using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FCDBApp.Models
{
    public class InspectionTable
    {
        [Key]
        public Guid InspectionID { get; set; }
        public string Branch { get; set; }
        public string VehicleReg { get; set; }
        public string VehicleType { get; set; }
        public DateTime InspectionDate { get; set; }
        public DateTime NextInspectionDue { get; set; }
        public DateTime SubmissionTime { get; set; } = DateTime.Now;
        public int InspectionTypeID { get; set; }
        public int? SiteID { get; set; }

        public ICollection<InspectionDetails> Details { get; set; } = new List<InspectionDetails>();
        public string PassFailStatus { get; set; }
    }

    public class InspectionDetails
    {
        [Key]
        public int InspectionDetailID { get; set; }

        [ForeignKey("InspectionTable")]
        public Guid InspectionID { get; set; }
        public int InspectionItemID { get; set; }
        public string Result { get; set; }
        public string Comments { get; set; }

        public InspectionTable Inspection { get; set; }
        public InspectionItems Item { get; set; }
    }

    public class InspectionItems
    {
        [Key]
        public int InspectionItemID { get; set; }
        public int CategoryID { get; set; }
        public string ItemDescription { get; set; }
        public string InspectionTypeIndicator { get; set; }
        public int InspectionTypeID { get; set; }

        public InspectionCategories Category { get; set; }
        public InspectionType InspectionType { get; set; }
    }

    public class InspectionCategories
    {
        [Key]
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public ICollection<InspectionItems> Items { get; set; }
    }

    public class InspectionType
    {
        [Key]
        public int InspectionTypeID { get; set; }
        public string TypeName { get; set; }
        public string Frequency { get; set; }

        public ICollection<InspectionItems> Items { get; set; }
    }


    // DTO classes for data transfer and local storage
    public class InspectionTableDto
    {
        public Guid InspectionID { get; set; }
        public string Branch { get; set; }
        public string VehicleReg { get; set; }
        public string VehicleType { get; set; }
        public DateTime InspectionDate { get; set; }
        public DateTime NextInspectionDue { get; set; }
        public DateTime SubmissionTime { get; set; } = DateTime.Now;
        public int InspectionTypeID { get; set; }
        public int? SiteID { get; set; }
        public ICollection<InspectionDetailsDto> Details { get; set; } = new List<InspectionDetailsDto>();
        public string PassFailStatus { get; set; }
    }

    public class InspectionDetailsDto
    {
        public int InspectionDetailID { get; set; }
        public Guid InspectionID { get; set; }
        public int InspectionItemID { get; set; }
        public string Result { get; set; }
        public string Comments { get; set; }
    }

    public class InspectionItemDto
    {
        public int InspectionItemID { get; set; }
        public string ItemDescription { get; set; }
        public int CategoryID { get; set; }
        public string InspectionTypeIndicator { get; set; }
        public int InspectionTypeID { get; set; }
    }

    public class InspectionCategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public List<InspectionItemDto> Items { get; set; } = new List<InspectionItemDto>();
    }
    public class Site
    {
        public int SiteID { get; set; }
        public string SiteName { get; set; }
    }

    public class Document
    {
        public int DocumentID { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public DateTime UploadDate { get; set; }
        public int DocumentCategoryID { get; set; }
        public DocumentCategory DocumentCategory { get; set; }
        public string? Notes { get; set; }
    }


    public class DocumentCategory
    {
        public int DocumentCategoryID { get; set; }
        public string CategoryName { get; set; }
        public ICollection<Document> Documents { get; set; }
    }

}
