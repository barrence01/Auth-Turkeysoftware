using System.Text.Json.Serialization;

namespace Auth_Turkeysoftware.Models.Response
{
    public class Response<T>
    {
        public int Status { get; }

        public string Title { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; }

        public Response(int status, string title, string? message, List<string>? errors, T? data)
        {
            Status = status;
            Title = title;
            Message = message;
            Errors = errors;
            Data = data;
        }
    }
}
