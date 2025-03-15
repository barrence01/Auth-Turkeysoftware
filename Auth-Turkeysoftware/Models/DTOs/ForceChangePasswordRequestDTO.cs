using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.DTOs
{
    public class ForceChangePasswordRequestDTO
    {
        [Required(ErrorMessage = "É necessário fornecer um email de usuário")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "É necessário fornecer uma nova senha")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "É necessário repetir a senha")]
        [Compare(nameof(NewPassword), ErrorMessage = "As senhas não combinam")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
