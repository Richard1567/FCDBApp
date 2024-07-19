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

        [Required]
        public string Branch { get; set; }

        [Required]
        public string VehicleReg { get; set; }

        [Required]
        public string VehicleType { get; set; }

        public int Odometer { get; set; }

        [Required]
        public DateTime InspectionDate { get; set; }

        public DateTime NextInspectionDue { get; set; }

        public DateTime SubmissionTime { get; set; }

        [Required]
        public int InspectionTypeID { get; set; }

        public int? SiteID { get; set; }

        public string PassFailStatus { get; set; }

        public ICollection<InspectionDetails> Details { get; set; } = new List<InspectionDetails>();

        // Foreign keys to Signatures
        public Guid? EngineerSignatureID { get; set; }

        public Guid? BranchManagerSignatureID { get; set; }

        // These fields will not be mapped to the database
        [NotMapped]
        public Signature EngineerSignature { get; set; }

        [NotMapped]
        public Signature BranchManagerSignature { get; set; }

        [NotMapped]
        public string EngineerSignatureBase64 { get; set; }

        [NotMapped]
        public string BranchManagerSignatureBase64 { get; set; }

        [NotMapped]
        public string EngineerPrint { get; set; }

        [NotMapped]
        public string BranchManagerPrint { get; set; }
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
        public int Odometer { get; set; }
        public DateTime InspectionDate { get; set; }
        public DateTime NextInspectionDue { get; set; }
        public DateTime SubmissionTime { get; set; } = DateTime.Now;
        public int InspectionTypeID { get; set; }
        public int? SiteID { get; set; }
        public ICollection<InspectionDetailsDto> Details { get; set; } = new List<InspectionDetailsDto>();
        public string PassFailStatus { get; set; }
        public Guid? EngineerSignatureID { get; set; }
        public Guid? BranchManagerSignatureID { get; set; }

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
        public byte[] DocumentData { get; set; }  // Store binary data
        public string ContentType { get; set; }  // Store the content type
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

    public class TemplateFile
    {
        [Key]
        public int TemplateID { get; set; }

        [Required]
        [StringLength(255)]
        public string TemplateName { get; set; }

        [Required]
        public byte[] TemplateData { get; set; }

        [Required]
        [StringLength(255)]
        public string ContentType { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;
    }



}
