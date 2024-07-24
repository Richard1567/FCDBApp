using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using FCDBApp.Models;

namespace FCDBApp.Models
{
    public class JobCard
    {
        public Guid JobCardID { get; set; }
        public string? JobNo { get; set; } = string.Empty;
        public string Site { get; set; } = string.Empty;
        public int? SiteID { get; set; }
        public string Engineer { get; set; } = string.Empty;
        public string? RegNo { get; set; }
        public string? CustOrderNo { get; set; }
        public uint? Odometer { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public DateTime SubmissionTime { get; set; }
        public string? Description { get; set; }
        public List<PartUsed> PartsUsed { get; set; } = new List<PartUsed>();

        [Timestamp]
        public byte[] RowVersion { get; set; } = new byte[0];

        // Link to JobCardSignature
        public Guid? EngineerSignatureID { get; set; }
        public Guid? BranchManagerSignatureID { get; set; }

        // Unmapped properties for temporary storing signatures
        [NotMapped]
        public string? TempEngineerSignature { get; set; }
        [NotMapped]
        public string? TempBranchManagerSignature { get; set; }
        [NotMapped]
        public string StartTime { get; set; }
        [NotMapped]
        public string EndTime { get; set; }
    }


}
