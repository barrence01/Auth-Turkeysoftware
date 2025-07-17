namespace Auth_Turkeysoftware.Domain.Models.VOs
{
    public class SendEmailVO
    {
        public List<string> To { get; set; } = new List<string>();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
