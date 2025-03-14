using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; }
    }
}
