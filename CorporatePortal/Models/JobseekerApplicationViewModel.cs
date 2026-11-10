using System;

namespace CorporatePortal.Models
{
    public class JobseekerApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; }
    }
}
