using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Test.Repositories.Models;
using Laraue.EfCoreTriggers.Common.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Auth_Turkeysoftware.Repositories.Context
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
                            action.Insert<HistUserLoginModel>(userSessionModel => new HistUserLoginModel
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
            // TODO: Adicionar cache de 10 para esta trigger
            ////
            builder.HasSequence<int>("hist_aspnet_users_sequence")
                   .StartsAt(1)
                   .IncrementsBy(1);

            builder.Entity<HistAplicationUserModel>()
                   .Property(e => e.HistoryId)
                   .IsRequired()
                   .HasDefaultValueSql("nextval('\"hist_aspnet_users_sequence\"')");

            builder.Entity<ApplicationUser>()
                   .AfterInsert(trigger =>
                       trigger.Action(action =>
                            action.Insert<HistAplicationUserModel>(applicationUser => new HistAplicationUserModel
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
                            action.Insert<HistAplicationUserModel>(applicationUser => new HistAplicationUserModel
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


            ////
            // TABLE: tb_log_admin_action
            // MODEL: AdminActionLogModel
            ////
            builder.HasSequence<long>("admin_action_sequence")
                   .StartsAt(1)
                   .IncrementsBy(1);

            builder.Entity<AdminActionLogModel>()
                   .Property(e => e.AdminActionId)
                   .HasDefaultValueSql("nextval('\"admin_action_sequence\"')");

            ////
            // TABLE: tb_two_factor_auth
            // MODEL: TwoFactorAuthModel
            ////
            builder.HasSequence<long>("two_factor_auth_sequence")
                   .StartsAt(1)
                   .IncrementsBy(1);

            builder.Entity<TwoFactorAuthModel>()
                   .Property(e => e.TwoFactorId)
                   .HasDefaultValueSql("nextval('\"two_factor_auth_sequence\"')");

            // Declaração de chave composta
            builder.Entity<TwoFactorAuthModel>()
                        .HasKey(e => new { e.FkUserId, e.TwoFactorMode });

            builder.Entity<TwoFactorAuthModel>()
                   .HasOne(e => e.User)
                   .WithMany(f => f.Registered2FAModes)
                   .HasForeignKey(e => e.FkUserId)
                   .OnDelete(DeleteBehavior.Cascade);

            ////
            // TABLE: tb_test
            // MODEL: TestDataModel
            ////
            builder.HasSequence<int>("test_data_sequence")
                   .StartsAt(1)
                   .IncrementsBy(1);

            builder.Entity<TestDataModel>()
                   .Property(e => e.IdTest)
                   .HasDefaultValueSql("nextval('\"test_data_sequence\"')");


            // Para criar um owner/schema
            //modelBuilder.Entity<Customer>()
            //            .ToTable("table_name", "owner");

            base.OnModelCreating(builder);
        }
    }
}
