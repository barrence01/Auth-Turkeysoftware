namespace Auth_Turkeysoftware.API.Models.Response
{
    public class UserAccountStatusResponse
    {
        public string Email { get; set; }

        public bool IsBloqueado { get; set; }

        public UserAccountStatusResponse(string email, bool isBloqueado)
        {
            Email = email;
            IsBloqueado = isBloqueado;

        }
    }
}
