using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.Request
{
    public class EnableTwoFactorRequest
    {
        [Required(ErrorMessage = "Modo de operação é obrigatório")]
        public int OperationMode { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Password { get; set; } = string.Empty;
    }
}
