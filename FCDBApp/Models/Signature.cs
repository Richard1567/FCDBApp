namespace FCDBApp.Models
{


        public class Signature
        {
            public Guid SignatureID { get; set; }
            public byte[] SignatureImage { get; set; }
            public string Print { get; set; } // Added field for print
            public string SignatoryType { get; set; } // e.g., 'Engineer', 'BranchManager'
            public DateTime CreatedAt { get; set; } = DateTime.Now;
        }




    public class SignatureDto
    {
        public Guid SignatureID { get; set; }
        public string Print { get; set; }
        public byte[] SignatureImage { get; set; }
        public string SignatoryType { get; set; }
    }


}
