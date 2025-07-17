using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;



//TODO Alterar o username para ser email
namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities
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
        public ApplicationUser()
        {
            Id = Guid.CreateVersion7().ToString();
            SecurityStamp = Guid.CreateVersion7().ToString();
            Name = string.Empty;
        }

        [MaxLength(128)]
        [Required]
        public string Name { get; set; }

        public ICollection<TwoFactorAuthModel>? Registered2FAModes { get; set; }
    }
}
