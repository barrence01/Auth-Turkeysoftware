namespace Auth_Turkeysoftware.API.Models.Response
{
    public class TryLoginResponse
    {
        public bool IsAccountLockedOut { get; set; } = false;
        public bool IsTwoFactorRequired { get; set; } = false;
        public bool IsPasswordEmailInvalid { get; set; } = false;
        public bool IsSuccess { get; set; } = false;
    }
}
