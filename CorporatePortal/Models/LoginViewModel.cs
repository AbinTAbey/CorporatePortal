using System.ComponentModel.DataAnnotations;

namespace CorporatePortal.Models
{
    public class LoginViewModel
    {
        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
