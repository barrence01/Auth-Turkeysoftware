using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.Request
{
    public class LoginRequest
    {
        private string _email = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email
        {
            get => _email;
            set => _email = value.ToLower();
        }

        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; } = string.Empty;

        public string? TwoFactorCode { get; set; } = string.Empty;
    }
}
