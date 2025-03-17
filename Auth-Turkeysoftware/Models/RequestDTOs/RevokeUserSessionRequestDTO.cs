using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.RequestDTOs
{
    public class RevokeUserSessionRequestDTO
    {
        private string _email = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email
        {
            get => _email;
            set => _email = value?.ToLower();
        }

        public string UserSessionId { get; set; } = string.Empty;
    }
}
