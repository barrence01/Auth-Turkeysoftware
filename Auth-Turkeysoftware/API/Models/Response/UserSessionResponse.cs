namespace Auth_Turkeysoftware.API.Models.Response
{
    public class UserSessionResponse
    {
        public string? SessionId { get; set; }

        public char? TokenStatus { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? LastTimeOnline { get; set; }

        public string? Platform { get; set; }

        public string? Country { get; set; }

        public string? UF { get; set; }

        public string? IP { get; set; }
    }
}
