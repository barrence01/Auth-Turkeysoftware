using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Repositories.Context
{
    public class CacheDbContext : DbContext
    {
        public CacheDbContext(DbContextOptions<CacheDbContext> options) : base(options) { }

        public DbSet<CacheEntryModel> DistributedCache { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
