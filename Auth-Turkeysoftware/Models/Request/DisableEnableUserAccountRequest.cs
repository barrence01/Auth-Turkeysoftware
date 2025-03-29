using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.Request
{
    public class DisableEnableUserAccountRequest
    {
        private string _email = string.Empty;

        [Required(ErrorMessage = "É necessário fornecer um email de usuário")]
        public string Email
        {
            get => _email;
            set => _email = value?.ToLower();
        }

        public int operationMode { get; set; } = 0;
    }
}
