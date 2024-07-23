using FCDBApp.Models;
using System.ComponentModel.DataAnnotations;

namespace FCDBApp.Models
{
    public class PartUsed
    {
        public Guid PartUsedID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public Guid JobCardID { get; set; } // Foreign key
        //public JobCard JobCard { get; set; } // Navigation Property
    }
    public class PartsList
    {
        [Key]
        public Guid PartsListID { get; set; }

        [Required]
        public string Name { get; set; }

        public string PartNumber { get; set; }
    }
    public class ManagePartsUsedViewModel
    {
        public string Engineer { get; set; }
        public List<PartUsedDetails> PartsUsed { get; set; } = new List<PartUsedDetails>();
    }

    public class PartUsedDetails
    {
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public int Quantity { get; set; }
        public string JobRegNo { get; set; }
        public DateTime JobDate { get; set; }
    }
}
