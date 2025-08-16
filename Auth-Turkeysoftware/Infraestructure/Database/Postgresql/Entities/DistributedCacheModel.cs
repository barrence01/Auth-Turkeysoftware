using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities
{
    [Table("DistributedCache", Schema = "auth")]
    public class DistributedCacheModel
    {
        [Key]
        [MaxLength(255)]
        public string Id { get; set; } = null!;

        [Required]
        public byte[] Value { get; set; } = null!;

        [Required]
        public DateTimeOffset ExpiresAtTime { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        public DateTimeOffset? AbsoluteExpiration { get; set; }
    }

    public class CacheEntryOptions
    {
        public TimeSpan? SlidingExpiration { get; init; }

        public DateTimeOffset? AbsoluteExpiration { get; init; }
    }
}
