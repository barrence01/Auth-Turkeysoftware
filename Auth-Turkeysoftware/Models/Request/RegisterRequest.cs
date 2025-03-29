using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.Request
{
    public class RegisterRequest
    {
        private string _email = string.Empty;

        [EmailAddress]
        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email
        {
            get => _email;
            set => _email = value?.ToLower();
        }

        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório")]
        public string PhoneNumber { get; set; }
    }
}
