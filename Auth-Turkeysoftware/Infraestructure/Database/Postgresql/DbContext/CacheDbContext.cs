using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.DbContext
{
    public class CacheDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public CacheDbContext(DbContextOptions<CacheDbContext> options) : base(options) { }

        public DbSet<DistributedCacheModel> DistributedCache { get; set; }
        public DbSet<LogAdminActionModel> AdminActionLog { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
