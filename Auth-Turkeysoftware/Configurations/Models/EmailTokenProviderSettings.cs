namespace Auth_Turkeysoftware.Configurations.Models
{
    public class EmailTokenProviderSettings
    {
        public TimeSpan TokenLifeSpan { get; init; } = TimeSpan.FromMinutes(10);
        public int TokenLength { get; init; } = 8;
        public int TokenInitialRange { get; init; } = 10000000;
        public int TokenFinalRange { get; init; } = 99999999;
        public string TokenFormat { get; init; } = "D8";
        public int MaxNumberOfTries { get; init; } = 15;
    }
}
