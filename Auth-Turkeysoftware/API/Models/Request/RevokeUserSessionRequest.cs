using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.API.Models.Request
{
    public class RevokeUserSessionRequest
    {
        private string _email = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email
        {
            get => _email;
            set => _email = value.ToLower();
        }

        public string SessionId { get; set; } = string.Empty;
    }
}
