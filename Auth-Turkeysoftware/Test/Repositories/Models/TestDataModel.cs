using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth_Turkeysoftware.Test.Repositories.Models
{
    [Table("tb_test", Schema = "auth")]
    public class TestDataModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_test", TypeName = "uuid")]
        public string? IdTest { get; set; }

        [Column("nr_number")]
        public int number { get; set; } = 324;

        [Column("ds_string")]
        public string dsString { get; set; } = "547";

        [NotMapped]
        public int teste_var { get; set; }
    }
}
