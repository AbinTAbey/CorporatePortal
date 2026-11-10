using System;
using System.Collections.Generic;

namespace CorporatePortal.Models
{
    public class MyJobsViewModel
    {
        public IList<MyJobItemViewModel> Jobs { get; set; }
        public string ActiveTab { get; set; }
        public int AppliedCount { get; set; }
        public int SavedCount { get; set; }
        public int InterviewCount { get; set; }
        public int ArchivedCount { get; set; }
    }

    public class MyJobItemViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string JobType { get; set; }
        public string SalaryRange { get; set; }
        public DateTime? AppliedDate { get; set; }
        public string Status { get; set; }
        public bool IsApplied { get; set; }
        public bool IsSaved { get; set; }
        public bool IsArchived { get; set; }
    }
}
