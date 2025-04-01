using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    [Table("tb_log_admin_action")]
    public class AdminActionLogModel
    {
        [Key]
        [Column("id_action")]
        public long? IdAction { get; set; }

        [Column("fk_id_usuario")]
        [MaxLength(255)]
        [Required]
        public string? FkIdUsuario { get; set; }

        [Column("nm_classe_metodo_executado")]
        [MaxLength(255)]
        [Required]
        public string? NomeMetodo { get; set; }

        [Column("ds_argumentos", TypeName = "text")]
        [Required]
        public string? Argumentos { get; set; }

        [Column("dt_inclusao")]
        [Required]
        public DateTime? DataInclusao { get; set; }

        public AdminActionLogModel() { }

        public AdminActionLogModel(string idAdmin, string nomeMetodo, string argumentos)
        {
            FkIdUsuario = idAdmin;
            NomeMetodo = nomeMetodo;
            Argumentos = argumentos;
            DataInclusao = DateTime.Now.ToUniversalTime();
        }
    }
}
