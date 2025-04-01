namespace Auth_Turkeysoftware.Models.DTOs
{
    public class TwoFactorDTO
    {
        public required string TwoFactorCode { get; set; }
        public int NumberOfTries { get; set; } = 0;

        public TwoFactorDTO() { }
    }
}
