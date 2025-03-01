using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

//TODO Alterar o username para ser email
namespace Auth_Turkeysoftware.Models.DataBaseModels
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
        [MaxLength(120)]
        [Column(TypeName = "VARCHAR")]
        public string? Name { get; set; }
    }
}
