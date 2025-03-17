using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.RequestDTOs
{
    public class RegisterRequestDTO
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
        public string? Password { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
