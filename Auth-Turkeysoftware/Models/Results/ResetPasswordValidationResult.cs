using Microsoft.AspNetCore.Identity;

namespace Auth_Turkeysoftware.Models.Results
{
    public class ResetPasswordValidationResult
    {
        public bool IsResetCodeEmpty { get; set; } = false;
        public bool IsNewPasswordEmpty { get; set; } = false;
        public bool IsResetCodeExpired { get; set; } = false;

        public List<IdentityError> Errors { get; set; } = new List<IdentityError>();

        public bool HasSucceeded() { 
            return !IsResetCodeEmpty && !IsNewPasswordEmpty && !IsResetCodeExpired && Errors.Count == 0;
        }
    }
}
