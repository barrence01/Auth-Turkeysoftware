using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    [Table("tb_hist_usuar_login", Schema = "auth")]
    [Keyless]
    public class HistUserLoginModel
    {
        [Column("id_sessao")]
        [Required]
        public string? IdSessao { get; set; }

        [Column("fk_id_usuario")]
        [Required]
        [MaxLength(255)]
        public string? FkIdUsuario { get; set; }

        [Column("dt_inclusao")]
        [Required]
        public DateTime? DataInclusao { get; set; }

        [Column("nm_pais")]
        [MaxLength(60)]
        public string? Pais { get; set; }

        [Column("nm_estado")]
        [MaxLength(60)]
        public string? UF { get; set; }

        [Column("nm_provedora")]
        [MaxLength(60)]
        public string? Provedora { get; set; }

        [Column("nr_ip")]
        [Required]
        [MaxLength(50)]
        public string? IP { get; set; }

        [Column("nm_platform")]
        [MaxLength(30)]
        public string? Platforma { get; set; }

        [Column("ds_user_agent")]
        [MaxLength(150)]
        public string? UserAgent { get; set; }

        [Required]
        public char DbOperationType { get; set; }

        [Required]
        public DateTime? DbOperationWhen { get; set; }
    }
}
