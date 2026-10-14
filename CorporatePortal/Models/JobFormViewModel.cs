using System.ComponentModel.DataAnnotations;

namespace CorporatePortal.Models
{
    public class JobFormViewModel
    {
        public int? JobId { get; set; }
        [Required, StringLength(200)] public string JobTitle { get; set; }
        [StringLength(100)] public string JobType { get; set; }
        [StringLength(100)] public string ExperienceRequired { get; set; }
        [StringLength(200)] public string Location { get; set; }
        [StringLength(100)] public string SalaryRange { get; set; }
        public string RequiredSkills { get; set; }
        public string JobDescription { get; set; }
    }
}
