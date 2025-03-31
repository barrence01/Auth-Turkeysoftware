namespace Auth_Turkeysoftware.Models.Response
{
    public class LoginResponse
    {
        public bool IsTwoFactorRequired { get; set; } = false;
        public bool HasTwoFactorCodeExpired { get; set; } = false;
        public bool HasTwoFactorFailed { get; set; } = false;
        public bool IsAccountLockedOut { get; set; } = false;
        public bool HasSucceeded { get; set; } = false;
    }
}
