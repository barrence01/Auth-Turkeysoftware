namespace Auth_Turkeysoftware.Configurations.Models
{
    public class EmailTokenProviderSettings
    {
        public TimeSpan TokenLifeSpan { get; init; } = TimeSpan.FromMinutes(10);
        public int TokenLength { get; init; } = 8;
        public string TokenFormat { get; init; } = "D8";

        public int MaxNumberOfTries { get; init; } = 15;
    }
}
