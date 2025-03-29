using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Repositories.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserSessionModel> LoggedUser { get; set; }

        public DbSet<AdminActionLogModel> AdminActionLog { get; set; }

        public DbSet<TestDataModel> TestData { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasSequence<long>("admin_action_sequence")
                   .StartsAt(1)
                   .IncrementsBy(1);

            builder.Entity<AdminActionLogModel>()
                   .Property(e => e.IdAction)
                   .HasDefaultValueSql("nextval('\"admin_action_sequence\"')");


            builder.HasSequence<int>("test_data_sequence")
                   .StartsAt(1)
                   .IncrementsBy(1);
            builder.Entity<TestDataModel>()
                   .Property(e => e.IdTest)
                   .HasDefaultValueSql("nextval('\"test_data_sequence\"')");

            base.OnModelCreating(builder);
        }
    }
}
