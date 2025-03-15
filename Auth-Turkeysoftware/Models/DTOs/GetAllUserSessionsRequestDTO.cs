using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.DTOs
{
    public class GetAllUserSessionsRequestDTO
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email { get; set; }

        public int pagina { get; set; } = 1;
    }
}
