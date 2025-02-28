using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models
{
    public class RegisterModel
    {

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
