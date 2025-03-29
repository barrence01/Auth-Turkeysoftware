namespace Auth_Turkeysoftware.Models.Response
{
    public class UserSessionResponse
    {
        public string? IdSessao { get; set; }

        public char? TokenStatus { get; set; }

        public DateTime? DataInclusao { get; set; }

        public DateTime? UltimaVezOnline { get; set; }

        public string? Platform { get; set; }

        public string? Pais { get; set; }

        public string? UF { get; set; }

        public string? IP { get; set; }
    }
}
