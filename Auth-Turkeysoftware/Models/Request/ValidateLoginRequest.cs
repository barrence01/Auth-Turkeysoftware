using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Models.Request
{
    public class ValidateLoginRequest
    {
        public string? TwoFactorCode { get; set; } = string.Empty;

        public int TwoFactorMode { get; set; } = 0;
    }
}
