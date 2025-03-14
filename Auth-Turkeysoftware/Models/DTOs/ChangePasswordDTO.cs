using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "É necessário fornecer a senha atual")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "É necessário fornecer uma nova senha")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "É necessário repetir a senha")]
        [Compare(nameof(NewPassword), ErrorMessage = "As senhas não combinam")]
        public string NewPasswordRepeated { get; set; } = string.Empty;
    }
}
