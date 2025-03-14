using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.DTOs
{
    public class EmailRequestDTO
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email { get; set; }
    }
}
