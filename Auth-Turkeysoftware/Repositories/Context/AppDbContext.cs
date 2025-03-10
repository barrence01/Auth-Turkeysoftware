using Auth_Turkeysoftware.Models.DataBaseModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Repositories.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LoggedUserModel> LoggedUser { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.HasSequence<int>("usuario_logado_sequence")
                   //.IsCyclic()
                   //.StartsAt(1)
                   //.IncrementsBy(1);

            //builder.Entity<LoggedUserModel>()
                   //.Property(e => e.IdSessao)
                   //.HasDefaultValueSql("NEXT VALUE FOR usuario_logado_sequence");

            base.OnModelCreating(builder);
        }
    }
}
