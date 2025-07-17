using System.Text.Json.Serialization;

namespace Auth_Turkeysoftware.API.Models.Response
{
    public class Response<T>
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? Errors { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Payload { get; }

        public Response(string? message = default, Dictionary<string, string[]>? errors = default, T? payload = default)
        {
            Message = message;
            Errors = errors;
            Payload = payload;
        }
    }
}
