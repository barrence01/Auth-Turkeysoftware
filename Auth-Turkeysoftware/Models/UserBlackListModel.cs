using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth_Turkeysoftware.Models
{
    [Table("USER_BLACKLIST")]
    public class UserBlackListModel
    {
        [Key]
        [MaxLength(256)]
        [Column("ds_email", TypeName = "VARCHAR")]
        public string? EmailUsuario { get; set; }

        [Column("ds_refresh_token", TypeName = "longtext")]
        public string? RefreshToken { get; set; }

        [Column("dt_inclusao")]
        public DateTime? DataInclusao { get; set; }
    }
}
