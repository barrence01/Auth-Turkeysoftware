using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    [Table("TB_HIST_AspNetUsers")]
    [Comment("Tracks the changes in the AspNetUsers table")]
    [Keyless]
    public class HistAplicationUserModel
    {
        [Required]
        public string? Id { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? NormalizedUserName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? NormalizedEmail { get; set; }
        [Required]
        public string? PasswordHash { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }

        [Column(TypeName = "VARCHAR")]
        [MaxLength(128)]
        public string? Name { get; set; }

        [Required]
        public char? DbOperationType { get; set; }

        [Column("dt_inclusao")]
        [Required]
        public DateTime? DataInclusao { get; set; }
    }
}
