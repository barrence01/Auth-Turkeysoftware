namespace Auth_Turkeysoftware.Domain.Models.VOs
{
    public class TwoFactorRetryVO
    {
        public required string UserId { get; set; }
        public required string TwoFactorCode { get; set; }
        public int NumberOfTries { get; set; } = 0;
        public int MaxNumberOfTries { get; set; } = 5;
        public TwoFactorRetryVO() { }
    }
}
