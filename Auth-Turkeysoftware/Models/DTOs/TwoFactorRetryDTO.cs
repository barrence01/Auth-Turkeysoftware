namespace Auth_Turkeysoftware.Models.DTOs
{
    public class TwoFactorRetryDto
    {
        public required string UserId { get; set; }
        public required string TwoFactorCode { get; set; }
        public int NumberOfTries { get; set; } = 0;
        public int MaxNumberOfTries { get; set; } = 5;
        public TwoFactorRetryDto() { }
    }
}
