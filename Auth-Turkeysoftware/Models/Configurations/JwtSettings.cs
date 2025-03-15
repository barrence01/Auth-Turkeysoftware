namespace Auth_Turkeysoftware.Models.Configurations
{
    public class JwtSettings
    {
        public string AccessSecretKey { get; set; }
        public string RefreshSecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Domain { get; set; }
        public int AccessTokenValidityInMinutes { get; set; }
        public int RefreshTokenValidityInMinutes { get; set; }
        public string RefreshTokenPath { get; set; }
        public string AccessTokenPath { get; set; }
        //public Object[] JwtAuthorities { get; private set; }
    }
}
