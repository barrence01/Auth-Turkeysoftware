using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.DTOs
{
    public class DisableEnableUserAccountRequestDTO
    {
        [Required(ErrorMessage = "É necessário fornecer um email de usuário")]
        public string Email { get; set; } = string.Empty;

        public int operationMode { get; set; } = 0;
    }
}
