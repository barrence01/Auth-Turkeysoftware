using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    [Table("tb_log_admin_action")]
    public class AdminActionLogModel
    {
        [Key]
        [Column("id_admin_action")]
        public long? AdminActionId { get; set; }

        [Column("fk_id_usuario")]
        [Required]
        public string? FkUserId { get; set; }

        [Column("nm_class_metdo_exec")]
        [Required]
        [MaxLength(255)]
        public string? MethodName { get; set; }

        [Column("ds_args")]
        [Required]
        public string? MethodArguments { get; set; }

        [Column("dt_inclusao")]
        [Required]
        public DateTime? CreatedOn { get; set; }

        public AdminActionLogModel() { }

        public AdminActionLogModel(string idAdmin, string nomeMetodo, string argumentos)
        {
            FkUserId = idAdmin;
            MethodName = nomeMetodo;
            MethodArguments = argumentos;
            CreatedOn = DateTime.Now.ToUniversalTime();
        }
    }
}
