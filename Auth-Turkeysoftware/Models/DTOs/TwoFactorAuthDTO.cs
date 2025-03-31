namespace Auth_Turkeysoftware.Models.DTOs
{
    public class TwoFactorAuthDTO
    {
        public string TwoFactorCode { get; set; }
        public int NumberOfTries { get; set; } = 0;

        public TwoFactorAuthDTO(string code) {
            TwoFactorCode = code;
        }
    }
}
