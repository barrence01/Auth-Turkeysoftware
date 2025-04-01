using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core.Tokens;
using Microsoft.EntityFrameworkCore;
using Auth_Turkeysoftware.Extensions;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    [Table("tb_usuar_session", Schema = "auth")]
    [Index(nameof(FkIdUsuario), Name = "ix_fk_idusuar_session")]
    public class UserSessionModel
    {
        [Key]
        [Column("id_sessao")]
        public string IdSessao { get; set; }

        [Column("fk_id_usuario", TypeName = "VARCHAR")]
        [MaxLength(255)]
        [Required]
        public string? FkIdUsuario { get; set; }

        [Column("ds_refresh_token", TypeName = "text")]
        [Required]
        public string? RefreshToken { get; set; }

        [Column("st_token")]
        [AllowedValues('A', 'I')]
        [Required]
        [Comment("A - Ativo | I - Inativo")]
        public char? TokenStatus { get; set; }

        [Column("dt_inclusao")]
        [Required]
        public DateTime? DataInclusao { get; set; }

        [Column("dt_alteracao")]
        public DateTime? DataAlteracao { get; set; }

        [Column("nm_pais", TypeName = "VARCHAR")]
        [MaxLength(60)]
        public string? Pais { get; set; }

        [Column("nm_estado", TypeName = "VARCHAR")]
        [MaxLength(60)]
        public string? UF { get; set; }

        [Column("nm_provedora", TypeName = "VARCHAR")]
        [MaxLength(60)]
        public string? Provedora { get; set; }

        [Column("nr_ip", TypeName = "VARCHAR")]
        [MaxLength(50)]
        [Required]
        public string? IP { get; set; }

        [Column("nm_platform", TypeName = "VARCHAR")]
        [MaxLength(30)]
        public string? Platform { get; set; }

        [Column("ds_userAgent", TypeName = "VARCHAR")]
        [MaxLength(150)]
        public string? UserAgent { get; set; }

        public UserSessionModel() { }

        public bool IsValidForInclusion()
        {
            if (FkIdUsuario == null ||
                string.IsNullOrEmpty(RefreshToken) || string.IsNullOrEmpty(IP))
            {
                return false;
            }
            this.TruncateAllFields();
            return true;
        }
    }
}
