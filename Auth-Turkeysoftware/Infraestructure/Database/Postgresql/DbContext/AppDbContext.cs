using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.Test.Repositories.Models;
using Laraue.EfCoreTriggers.Common.Extensions;
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

            ////
            // TABLE: tb_usuar_session
            // MODEL: UserSessionModel
            ////
            builder.Entity<UserSessionModel>()
                   .AfterInsert(trigger =>
                       trigger.Action(action =>
                            action.Insert(userSessionModel => new HistUserLoginModel
                            {
                                SessionId = userSessionModel.New.SessionId,
                                FkUserId = userSessionModel.New.FkUserId,
                                CreatedOn = userSessionModel.New.CreatedOn,
                                UF = userSessionModel.New.UF,
                                ServiceProvider = userSessionModel.New.ServiceProvider,
                                IP = userSessionModel.New.IP,
                                Platform = userSessionModel.New.Platform,
                                UserAgent = userSessionModel.New.UserAgent,
                                DbOperationType = (char)DbOperationTypeEnum.INCLUSAO,
                                DbOperationWhen = DateTime.Now
                            })));

            ////
            // TABLE: tb_hist_aspnet_users
            // MODEL: HistAplicationUserModel
            ////
            builder.Entity<ApplicationUser>()
                   .AfterInsert(trigger =>
                       trigger.Action(action =>
                            action.Insert(applicationUser => new HistAplicationUserModel
                            {
                                UserId = applicationUser.New.Id,
                                UserName = applicationUser.New.UserName,
                                NormalizedUserName = applicationUser.New.NormalizedUserName,
                                Email = applicationUser.New.Email,
                                NormalizedEmail = applicationUser.New.NormalizedEmail,
                                PasswordHash = applicationUser.New.PasswordHash,
                                PhoneNumber = applicationUser.New.PhoneNumber,
                                Name = applicationUser.New.Name,
                                DbOperationType = (char)DbOperationTypeEnum.INCLUSAO,
                                DbOperationWhen = DateTime.Now
                            })))
                   .AfterUpdate(trigger =>
                        trigger.Action(action =>
                            action.Insert(applicationUser => new HistAplicationUserModel
                            {
                                UserId = applicationUser.New.Id,
                                UserName = applicationUser.New.UserName,
                                NormalizedUserName = applicationUser.New.NormalizedUserName,
                                Email = applicationUser.New.Email,
                                NormalizedEmail = applicationUser.New.NormalizedEmail,
                                PasswordHash = applicationUser.New.PasswordHash,
                                PhoneNumber = applicationUser.New.PhoneNumber,
                                Name = applicationUser.New.Name,
                                DbOperationType = (char)DbOperationTypeEnum.ALTERACAO,
                                DbOperationWhen = DateTime.Now
                            })));

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
    }
}
