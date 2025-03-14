using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.DTOs
{
    public class RegisterDTO
    {

        [EmailAddress]
        [Required(ErrorMessage = "Email é obrigatório")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password é obrigatório")]
        public string? Password { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
