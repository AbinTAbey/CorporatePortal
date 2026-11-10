using System;

namespace CorporatePortal.Models
{
    public class EmployerApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public int JobseekerId { get; set; }
        public string JobseekerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; }
        public string ResumePath { get; set; }
        public string CoverLetter { get; set; }
        public string Qualification { get; set; }
        public string Skills { get; set; }
        public string Experience { get; set; }
    }
}
