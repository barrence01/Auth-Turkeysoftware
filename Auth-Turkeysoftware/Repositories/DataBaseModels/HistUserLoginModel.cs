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

        [Column("fk_id_usuario", TypeName = "VARCHAR")]
        [MaxLength(255)]
        [Required]
        public string? FkIdUsuario { get; set; }

        [Column("dt_inclusao")]
        [Required]
        public DateTime? DataInclusao { get; set; }

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

        [Required]
        public char DbOperationType { get; set; }

        [Required]
        public DateTime? DbOperationWhen { get; set; }
    }
}
