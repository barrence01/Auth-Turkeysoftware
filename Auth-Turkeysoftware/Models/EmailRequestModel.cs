namespace Auth_Turkeysoftware.Models
{
    public class EmailRequestModel
    {
        public List<string> To { get; set; } = new List<string>();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
