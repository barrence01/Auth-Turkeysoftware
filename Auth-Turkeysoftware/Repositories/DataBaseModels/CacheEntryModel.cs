using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    [Table("DistributedCache", Schema = "auth")]
    public class CacheEntryModel
    {
        [Key]
        [MaxLength(255)]
        public string Id { get; set; } = null!;

        [Required]
        public byte[] Value { get; set; } = null!; 

        [Required]
        public DateTimeOffset ExpiresAtTime { get; set; }

        public long? SlidingExpiration { get; set; } 

        public DateTimeOffset? AbsoluteExpiration { get; set; } 
    }
}
