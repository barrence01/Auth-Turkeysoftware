namespace Auth_Turkeysoftware.Configurations.Models
{
    public class JwtSettings
    {
        public required string LoginSecretKey { get; init; }
        public required string AccessSecretKey { get; init; }
        public required string RefreshSecretKey { get; init; }
        public required string Issuer { get; init; }
        public string Audience { get; init; } = string.Empty;
        public string Domain { get; init; } = string.Empty;
        public int AccessTokenValidityInMinutes { get; init; } = 10;
        public int RefreshTokenValidityInMinutes { get; init; } = 10080;
        public required string RefreshTokenPath { get; init; }
        public required string AccessTokenPath { get; init; }
    }
}