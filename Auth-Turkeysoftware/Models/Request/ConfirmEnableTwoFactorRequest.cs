using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.Request
{
    public class ConfirmEnableTwoFactorRequest
    {
        [Required(ErrorMessage = "Modo de operação é obrigatório")]
        public int OperationMode { get; set; }

        [Required(ErrorMessage = "O código de 2 fatores é obrigatório")]
        public string Code { get; set; } = null!;

    }
}
