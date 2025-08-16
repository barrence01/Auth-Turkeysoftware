using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.Test.Repositories.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Auth_Turkeysoftware.Shared.Enums;
using Auth_Turkeysoftware.Domain.Models.VOs;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.DbContext
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
    {
        public DbSet<DistributedCacheModel> DistributedCache { get; set; }
        public DbSet<TwoFactorAuthModel> TwoFactorAuth { get; set; }
        public DbSet<UserSessionModel> LoggedUser { get; set; }
        public DbSet<LogAdminActionModel> AdminActionLog { get; set; }
        public DbSet<HistUserLoginModel> HistUserLogin { get; set; }
        public DbSet<HistAspNetUsersModel> HistAplicationUser { get; set; }
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
            var entriesAppUser = ChangeTracker.Entries<ApplicationUser>()
                                              .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified).ToList();

            foreach (var entry in entriesAppUser)
            {
                var historyEntry = new HistAspNetUsersModel
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

                this.Set<HistAspNetUsersModel>().Add(historyEntry);
            }

            // LOG of changes in UserSessionModel
            var entriesLogin = ChangeTracker.Entries<UserSessionModel>()
                                            .Where(e => e.State == EntityState.Added).ToList();

            foreach (var entry in entriesLogin)
            {
                var historyEntry = new HistUserLoginModel
                {
                    SessionId = entry.Entity.SessionId,
                    FkUserId = entry.Entity.FkUserId,
                    CreatedOn = entry.Entity.CreatedOn,
                    UF = entry.Entity.UF,
                    ServiceProvider = entry.Entity.ServiceProvider,
                    IP = entry.Entity.IP,
                    Platform = entry.Entity.Platform,
                    UserAgent = entry.Entity.UserAgent,
                    DbOperationType = (char)DbOperationTypeEnum.INCLUSAO,
                    DbOperationWhen = DateTime.Now.ToUniversalTime()
                };

                this.Set<HistUserLoginModel>().Add(historyEntry);
            }
        }
    }
}
