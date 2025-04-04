using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Auth_Turkeysoftware.Extensions
{
    public static class ModelStateExtensions
    {
        public static Dictionary<string, string[]>? ToErrorDictionaryOrNull(this ModelStateDictionary modelState)
        {
            if (modelState.IsValid) return null;

            return modelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );
        }
    }
}
