using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.Request
{
    public class ConfirmEmailRequest
    {
        [Required(ErrorMessage = "Usuário não identificado")]
        public required string UserId { get; init; }
        [Required(ErrorMessage = "Usuário não identificado")]
        public required string Email { get; init; }
        [Required(ErrorMessage = "Usuário não identificado")]
        public required string token { get; init; }
    }
}
