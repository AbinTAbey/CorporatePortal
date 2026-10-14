using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CorporatePortal.Models
{
    public class JobseekerProfileViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ResumePath { get; set; }
        public HttpPostedFileBase ResumeFile { get; set; }
        public string Qualification { get; set; }
        public string Skills { get; set; }
        public string Experience { get; set; }
        public string Address { get; set; }
    }
}
