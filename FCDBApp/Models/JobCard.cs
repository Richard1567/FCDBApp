using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FCDBApp.Models;

namespace FCDBApp.Models
{
    public class JobCard
    {
        public Guid JobCardID { get; set; }
        public string JobNo { get; set; } = string.Empty;
        public string Site { get; set; } = string.Empty;
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
    }



}
