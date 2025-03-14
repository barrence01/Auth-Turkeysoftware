using Auth_Turkeysoftware.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth_Turkeysoftware.Models.DataBaseModels
{
    [Table("TB_LOG_ADMIN_ACTION")]
    public class AdminActionLogModel
    {
        [Key]
        [Column("id_action")]
        public long? IdAction { get; set; }

        [Column("fk_id_usuario", TypeName = "VARCHAR")]
        [MaxLength(255)]
        [Required]
        public string FkIdUsuario { get; set; }

        [Column("nm_metodo_executado")]
        [MaxLength(128)]
        [Required]
        public string NomeMetodo { get; set; }

        [Column("ds_argumentos", TypeName = "text")]
        [Required]
        public string Argumentos { get; set; }

        [Column("dt_inclusao")]
        [Required]
        public DateTime? DataInclusao {  get; set; }

        public AdminActionLogModel() { }

        public AdminActionLogModel(string idAdmin, string metodo, string argumentos) {
            FkIdUsuario = idAdmin;
            NomeMetodo = metodo;
            Argumentos = argumentos;
            DataInclusao = DateTime.Now.ToUniversalTime();
        }
    }
}
