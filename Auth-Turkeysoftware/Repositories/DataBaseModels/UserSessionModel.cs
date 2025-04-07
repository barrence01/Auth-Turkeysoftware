using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core.Tokens;
using Microsoft.EntityFrameworkCore;
using Auth_Turkeysoftware.Extensions;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    [Table("tb_usuar_session", Schema = "auth")]
    [Index(nameof(FkUserId), Name = "IX_fk_id_usuar_session")]
    public class UserSessionModel
    {
        [Key]
        [Column("id_sessao")]
        public string SessionId { get; set; }

        [Column("fk_id_usuario")]
        [Required]
        public string? FkUserId { get; set; }

        [Column("ds_refresh_token")]
        [Required]
        public string? RefreshToken { get; set; }

        [Column("st_token")]
        [Required]
        [AllowedValues('A', 'I')]
        [Comment("A - Ativo | I - Inativo")]
        public char? TokenStatus { get; set; }

        [Column("dt_inclusao")]
        [Required]
        public DateTime? CreatedOn { get; set; }

        [Column("dt_alteracao")]
        public DateTime? UpdatedOn { get; set; }

        [Column("nm_pais")]
        [MaxLength(60)]
        public string? Country { get; set; }

        [Column("nm_estado")]
        [MaxLength(60)]
        public string? UF { get; set; }

        [Column("nm_provedora")]
        [MaxLength(60)]
        public string? ServiceProvider { get; set; }

        [Column("nr_ip")]
        [MaxLength(50)]
        [Required]
        public string? IP { get; set; }

        [Column("nm_platforma")]
        [MaxLength(30)]
        public string? Platform { get; set; }

        [Column("ds_user_agent")]
        [MaxLength(150)]
        public string? UserAgent { get; set; }

        public UserSessionModel() { }

        public bool IsValidForInclusion()
        {
            if (FkUserId == null ||
                string.IsNullOrEmpty(RefreshToken) || string.IsNullOrEmpty(IP))
            {
                return false;
            }
            this.TruncateAllFields();
            this.DataNormalizer();
            return true;
        }

        public void DataNormalizer()
        {
            Country = Country?.ToLowerInvariant();
            UF = UF?.ToLowerInvariant();
        }
    }
}
