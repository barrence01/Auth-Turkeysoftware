using Microsoft.AspNetCore.Identity;

namespace Auth_Turkeysoftware.Models
{
    /// <summary>
    /// ApplicationUser class
    /// <br></br>
    /// Extende a classe IdentityUser
    /// </summary>
    /// <remarks>
    /// SecurityStamp: É alterado sempre que o usuário altera as credenciais<br></br>
    /// ConcurrencyStamp: É alterado sempre que algum dado do usuário é alterado para evitar conflitos de concorrência.
    /// </remarks>
    public class ApplicationUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
