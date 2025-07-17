using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.API.Models.Request
{
    public class PasswordRequest
    {
        [Required(ErrorMessage = "A senha é inválida")]
        public string Password { get; set; } = string.Empty;
    }
}
