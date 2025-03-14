using Auth_Turkeysoftware.Models.DataBaseModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Repositories.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserSessionModel> LoggedUser { get; set; }

        public DbSet<AdminActionLogModel> AdminActionLog { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasSequence<long>("admin_action_sequence")
                   .StartsAt(1)
                   .IncrementsBy(1);

            builder.Entity<AdminActionLogModel>()
                   .Property(e => e.IdAction)
                   .HasDefaultValueSql("nextval('\"admin_action_sequence\"')");

            base.OnModelCreating(builder);
        }
    }
}
