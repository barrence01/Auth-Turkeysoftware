using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; }
    }
}
