using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.DTOs
{
    public class RevokeUserSessionDTO
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email { get; set; }

        public string UserSessionId { get; set; } = string.Empty;
    }
}
