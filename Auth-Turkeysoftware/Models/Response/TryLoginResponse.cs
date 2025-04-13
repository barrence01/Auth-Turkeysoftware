namespace Auth_Turkeysoftware.Models.Response
{
    public class TryLoginResponse
    {
        public bool IsAccountLockedOut { get; set; } = false;
        public bool IsTwoFactorRequired { get; set; } = false;
        public bool IsPasswordEmailInvalid { get; set; } = false;
    }
}
