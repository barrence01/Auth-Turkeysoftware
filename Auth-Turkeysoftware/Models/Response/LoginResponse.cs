namespace Auth_Turkeysoftware.Models.Response
{
    public class LoginResponse
    {
        public bool IsTwoFactorRequired { get; set; } = false;
        public bool IsTwoFactorCodeExpired { get; set; } = false;
        public bool IsTwoFactorCodeInvalid { get; set; } = false;
    }
}
