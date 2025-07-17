using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.API.Models.Request
{
    public class ListAllUserSessionsRequest
    {
        private string _email = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email
        {
            get => _email;
            set => _email = value.ToLower();
        }

        public int pagina { get; set; } = 1;
    }
}
