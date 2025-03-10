using System.Text.Json.Serialization;

namespace Auth_Turkeysoftware.Models
{
    public class Response
    {
        public string? Status { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Data { get; set; }
    }

    public static class MessageResponse
    {
        public const string Success = "Success";
        public const string Error = "Error";
    }
}
