using Microsoft.AspNetCore.Identity;

namespace Auth_Turkeysoftware.Models.Results
{
    public class RegisterUserResult()
    {
        public IEnumerable<IdentityError>? identityErrors { get; set; }
        public bool UserAlreadyExists { get; set; } = false;

        public bool HasExceptionThrow { get; set; } = false;

        public string ExceptionMessage { get; set; } = string.Empty;

        public bool HasSucceeded()
        {
            return !UserAlreadyExists && (identityErrors == null || !identityErrors.Any()) && !HasExceptionThrow;
        }
    }
}
