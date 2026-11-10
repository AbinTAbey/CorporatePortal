using System.ComponentModel.DataAnnotations;

namespace CorporatePortal.Models
{
    public class JobFormViewModel
    {
        public int? JobId { get; set; }
        [Required(ErrorMessage = "Job title is required."), StringLength(100, ErrorMessage = "Job title cannot exceed 100 characters.")]
        public string JobTitle { get; set; }
        [StringLength(50, ErrorMessage = "Job mode cannot exceed 50 characters.")]
        public string JobType { get; set; }
        [Required(ErrorMessage = "Experience required is required."), StringLength(50, ErrorMessage = "Experience required cannot exceed 50 characters.")]
        public string ExperienceRequired { get; set; }
        [Required(ErrorMessage = "Location is required."), StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string Location { get; set; }
        [StringLength(50, ErrorMessage = "Salary range cannot exceed 50 characters.")]
        public string SalaryRange { get; set; }
        [Required(ErrorMessage = "Required skills are required."), StringLength(500, ErrorMessage = "Required skills cannot exceed 500 characters.")]
        public string RequiredSkills { get; set; }
        [Required(ErrorMessage = "Job description is required.")]
        public string JobDescription { get; set; }
    }
}
