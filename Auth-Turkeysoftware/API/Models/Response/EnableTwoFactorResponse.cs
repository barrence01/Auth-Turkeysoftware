namespace Auth_Turkeysoftware.API.Models.Response
{
    public class EnableTwoFactorResponse
    {
        public bool IsPasswordInvalid { get; set; } = false;
        public bool IsEmailNotConfirmed { get; set; } = false;
        public bool IsTwoFactorCodeInvalid { get; set; } = false;
        public bool IsTwoFactorCodeExpired { get; set; } = false;
    }
}
