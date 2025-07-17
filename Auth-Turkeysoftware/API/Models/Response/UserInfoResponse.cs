namespace Auth_Turkeysoftware.API.Models.Response
{
    public class UserInfoResponse
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? PhoneConfirmed { get; set; }
        public bool? TwoFactorEnabled { get; set; }
    }
}
