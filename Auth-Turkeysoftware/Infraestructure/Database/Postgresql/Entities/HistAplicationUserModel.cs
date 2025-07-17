using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities
{
    [Table("tb_hist_aspnet_users", Schema = "auth")]
    [Comment("Tracks the changes in the AspNetUsers table")]
    public class HistAplicationUserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "uuid")]
        public Guid HistoryId { get; set; } = Guid.CreateVersion7();
        [Required]
        public string? UserId { get; set; }
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

        [Required]
        [MaxLength(128)]
        public string? Name { get; set; }

        [Required]
        public char? DbOperationType { get; set; }

        [Required]
        public DateTime? DbOperationWhen { get; set; }
    }
}
