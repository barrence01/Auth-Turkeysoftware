using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    [Table("TB_TEST")]
    public class TestDataModel
    {
        [Key]
        [Column("id_test")]
        public string IdTest { get; set; }

        [Column("nr_number")]
        public int number { get; set; } = 324;

        [Column("ds_string")]
        public string dsString { get; set; } = "547";

        [NotMapped]
        public int teste_var { get; set; }
    }
}
