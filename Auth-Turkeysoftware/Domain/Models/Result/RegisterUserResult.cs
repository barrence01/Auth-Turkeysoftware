using Microsoft.AspNetCore.Identity;

namespace Auth_Turkeysoftware.Domain.Models.Result
{
    public class RegisterUserResult() : IResult
    {
        public IEnumerable<IdentityError>? IdentityErrorList { get; set; }
        public bool UserAlreadyExists { get; set; } = false;

        public bool HasException { get; set; } = false;

        public string ExceptionMessage { get; set; } = string.Empty;

        public bool IsSuccess()
        {
            return !UserAlreadyExists && (IdentityErrorList == null || !IdentityErrorList.Any()) && !HasException;
        }
    }
}
