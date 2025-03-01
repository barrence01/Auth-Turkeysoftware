using Auth_Turkeysoftware.Models.DataBaseModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Auth_Turkeysoftware.Repositories.Context
{
    // Caso seja necessário criar mais de 1 DbContext
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ILogger _log = Log.ForContext(typeof(PathHelper));

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LoggedUserModel> LoggedUser { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasSequence<int>("usuario_logado_sequence")
                   .IsCyclic()
                   .StartsAt(1)
                   .IncrementsBy(1);

            builder.Entity<LoggedUserModel>()
                   .Property(e => e.IdSessao)
                   .HasDefaultValueSql("NEXT VALUE FOR usuario_logado_sequence");

            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Log.Warning);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
