using FCDBApp.Models;

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
}
