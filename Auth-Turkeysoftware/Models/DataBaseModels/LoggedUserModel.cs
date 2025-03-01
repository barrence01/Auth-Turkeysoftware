using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core.Tokens;
using Auth_Turkeysoftware.Extensions;

namespace Auth_Turkeysoftware.Models.DataBaseModels
{
    [Table("TB_USUAR_SESSION")]
    public class LoggedUserModel
    {
        [Key]
        [Column("id_sessao")]
        public int IdSessao { get; set; }

        [MaxLength(255)]
        [Column("fk_id_usuario", TypeName = "VARCHAR")]
        public string? FkIdUsuario { get; set; }

        [MaxLength(256)]
        [Column("ds_email", TypeName = "VARCHAR")]
        public string? EmailUsuario { get; set; }

        [Column("ds_refresh_token", TypeName = "longtext")]
        public string? RefreshToken { get; set; }

        [Column("st_token")]
        [AllowedValues('A', 'X')]
        public char? TokenStatus { get; set; } // A - Ativo | X - Cancelado

        [Column("dt_inclusao")]
        public DateTime? DataInclusao { get; set; }

        [Column("dt_alteracao")]
        public DateTime? DataAlteracao { get; set; }

        [MaxLength(60)]
        [Column("ds_pais", TypeName = "VARCHAR")]
        public string? Pais { get; set; }

        [MaxLength(60)]
        [Column("ds_estado", TypeName = "VARCHAR")]
        public string? UF { get; set; }

        [MaxLength(60)]
        [Column("ds_provedora", TypeName = "VARCHAR")]
        public string? Provedora { get; set; }

        [MaxLength(50)]
        [Column("ds_ip", TypeName = "VARCHAR")]
        public string? IP { get; set; }

        [MaxLength(30)]
        [Column("nm_device", TypeName = "VARCHAR")]
        public string? DeviceUsed { get; set; }

        public LoggedUserModel() { }

        public bool isValidForInclusion()
        {
            if (this.FkIdUsuario == null || string.IsNullOrEmpty(this.EmailUsuario) ||
                string.IsNullOrEmpty(this.RefreshToken) || string.IsNullOrEmpty(this.IP))
            {
                return false;
            }
            return true;
        }
    }
}
