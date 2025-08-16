using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.Test.Repositories.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Auth_Turkeysoftware.Shared.Enums;
using Auth_Turkeysoftware.Domain.Models.VOs;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.DbContext
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CacheEntryModel> DistributedCache { get; set; }
        public DbSet<TwoFactorAuthModel> TwoFactorAuth { get; set; }
        public DbSet<UserSessionModel> LoggedUser { get; set; }
        public DbSet<AdminActionLogModel> AdminActionLog { get; set; }
        public DbSet<HistUserLoginModel> HistUserLogin { get; set; }
        public DbSet<HistAplicationUserModel> HistAplicationUser { get; set; }
        public DbSet<TestDataModel> TestData { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            // Declaração de chave composta
            builder.Entity<TwoFactorAuthModel>()
                        .HasKey(e => new { e.FkUserId, e.TwoFactorMode });

            builder.Entity<TwoFactorAuthModel>()
                   .HasOne(e => e.User)
                   .WithMany(f => f.Registered2FAModes)
                   .HasForeignKey(e => e.FkUserId)
                   .OnDelete(DeleteBehavior.Cascade);


            // Para criar um owner/schema
            //modelBuilder.Entity<Customer>()
            //            .ToTable("table_name", "owner");

            base.OnModelCreating(builder);
        }

        public async Task<PaginationVO<T>> GetPagedResultAsync<T>(IQueryable<T> query, int pageNumber, int pageSize)
        {

            long totalCount = await query.CountAsync();

            int pageCount = (int)Math.Ceiling(totalCount / (double)pageNumber);

            if (totalCount <= 0 || pageNumber > pageCount)
            {
                return new PaginationVO<T>([], pageNumber, pageSize, totalCount);
            }

            var elements = await query
                                 .Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

            return new PaginationVO<T>
            {
                Data = elements,
                TotalCount = totalCount,
                PageCount = pageCount,
                PageSize = pageSize,
                PageNumber = pageNumber
            };
        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            HandleUserChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleUserChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void HandleUserChanges() {

            // LOG of changes in ApplicationUser
            var entries = ChangeTracker.Entries<ApplicationUser>()
                           .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var historyEntry = new HistAplicationUserModel
                {
                    UserId = entry.Entity.Id,
                    UserName = entry.Entity.UserName,
                    NormalizedUserName = entry.Entity.NormalizedUserName,
                    Email = entry.Entity.Email,
                    NormalizedEmail = entry.Entity.NormalizedEmail,
                    PasswordHash = entry.Entity.PasswordHash,
                    PhoneNumber = entry.Entity.PhoneNumber,
                    Name = entry.Entity.Name,
                    DbOperationType = entry.State == EntityState.Added
                                                    ? (char)DbOperationTypeEnum.INCLUSAO
                                                    : (char)DbOperationTypeEnum.ALTERACAO,
                    DbOperationWhen = DateTime.Now.ToUniversalTime()
                };

                this.Set<HistAplicationUserModel>().Add(historyEntry);
            }
        }
    }
}
