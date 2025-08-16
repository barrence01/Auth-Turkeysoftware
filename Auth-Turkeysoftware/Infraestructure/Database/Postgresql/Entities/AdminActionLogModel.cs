using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities
{
    [Table("tb_log_admin_action", Schema = "auth")]
    public class AdminActionLogModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_admin_action", TypeName = "uuid")]
        public Guid AdminActionId { get; set; } = Guid.CreateVersion7();

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
