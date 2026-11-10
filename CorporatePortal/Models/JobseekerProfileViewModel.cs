using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CorporatePortal.Models
{
    public class JobseekerProfileViewModel
    {
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }
        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters.")]
        public string Phone { get; set; }
        [StringLength(255, ErrorMessage = "Resume path cannot exceed 255 characters.")]
        public string ResumePath { get; set; }
        public HttpPostedFileBase ResumeFile { get; set; }
        [StringLength(150, ErrorMessage = "Qualification cannot exceed 150 characters.")]
        public string Qualification { get; set; }
        [StringLength(500, ErrorMessage = "Skills cannot exceed 500 characters.")]
        public string Skills { get; set; }
        [StringLength(50, ErrorMessage = "Experience cannot exceed 50 characters.")]
        public string Experience { get; set; }
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        public string Address { get; set; }
    }
}
