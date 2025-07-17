using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.API.Models.Request
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "É necessário fornecer a senha atual")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "É necessário fornecer uma nova senha")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "É necessário repetir a senha")]
        [Compare(nameof(NewPassword), ErrorMessage = "As senhas não combinam")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
