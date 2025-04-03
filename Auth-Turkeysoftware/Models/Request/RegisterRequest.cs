using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.Request
{
    public class RegisterRequest
    {
        private string _email = string.Empty;

        [EmailAddress(ErrorMessage = "O Email fornecido não é válido.")]
        [Required(ErrorMessage = "O Email é obrigatório")]
        public string Email
        {
            get => _email;
            set => _email = value.ToLower();
        }

        [Required(ErrorMessage = "É obrigatório fornecer uma senha")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "É obrigatório fornecer um nome")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "É obrigatório fornecer um telefone")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
