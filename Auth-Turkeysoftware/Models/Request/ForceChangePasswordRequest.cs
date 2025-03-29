using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.Request
{
    public class ForceChangePasswordRequest
    {
        private string _email = string.Empty;

        [Required(ErrorMessage = "É necessário fornecer um email de usuário")]
        public string Email
        {
            get => _email;
            set => _email = value?.ToLower();
        }

        [Required(ErrorMessage = "É necessário fornecer uma nova senha")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "É necessário repetir a senha")]
        [Compare(nameof(NewPassword), ErrorMessage = "As senhas não combinam")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
