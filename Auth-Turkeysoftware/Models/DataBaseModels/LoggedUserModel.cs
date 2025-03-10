using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core.Tokens;
using Microsoft.EntityFrameworkCore;
using Auth_Turkeysoftware.Extensions;

namespace Auth_Turkeysoftware.Models.DataBaseModels
{
    [Table("TB_USUAR_SESSION")]
    [Index(nameof(FkIdUsuario), Name = "IX_COD_USUAR_SESSION")]
    public class LoggedUserModel
    {
        [Key]
        [Column("id_sessao")]
        public string IdSessao { get; set; }

        [Column("fk_id_usuario", TypeName = "VARCHAR")]
        [MaxLength(255)]
        [Required]
        public string? FkIdUsuario { get; set; }

        [Column("ds_refresh_token", TypeName = "longtext")]
        [Required]
        public string? RefreshToken { get; set; }

        [Column("st_token")]
        [AllowedValues('A', 'I')]
        [Required]
        public char? TokenStatus { get; set; } // A - Ativa | I - Inativa

        [Column("dt_inclusao")]
        [Required]
        public DateTime? DataInclusao { get; set; }

        [Column("dt_alteracao")]
        public DateTime? DataAlteracao { get; set; }

        [Column("ds_pais", TypeName = "VARCHAR")]
        [MaxLength(60)]
        public string? Pais { get; set; }

        [Column("ds_estado", TypeName = "VARCHAR")]
        [MaxLength(60)]
        public string? UF { get; set; }

        [Column("ds_provedora", TypeName = "VARCHAR")]
        [MaxLength(60)]
        public string? Provedora { get; set; }

        [Column("ds_ip", TypeName = "VARCHAR")]
        [MaxLength(50)]
        [Required]
        public string? IP { get; set; }
        
        [Column("nm_platform", TypeName = "VARCHAR")]
        [MaxLength(30)]
        public string? Platform { get; set; }

        [Column("ds_userAgent", TypeName = "VARCHAR")]
        [MaxLength(150)]
        public string? UserAgent { get; set; }

        public LoggedUserModel() { }

        public bool IsValidForInclusion()
        {
            if (this.FkIdUsuario == null ||
                string.IsNullOrEmpty(this.RefreshToken) || string.IsNullOrEmpty(this.IP))
            {
                return false;
            }
            this.TruncateAllFields();
            return true;
        }
    }
}
