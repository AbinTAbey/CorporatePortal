using System.ComponentModel.DataAnnotations;

namespace CorporatePortal.Models
{
    public class EmployerProfileViewModel
    {
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }
        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters.")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Company name is required."), StringLength(150, ErrorMessage = "Company name cannot exceed 150 characters.")]
        public string CompanyName { get; set; }
        [StringLength(200, ErrorMessage = "Website cannot exceed 200 characters.")]
        public string Website { get; set; }
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        public string Address { get; set; }
        public string Description { get; set; }
    }
}
