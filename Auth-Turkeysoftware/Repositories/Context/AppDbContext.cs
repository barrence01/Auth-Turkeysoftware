using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Laraue.EfCoreTriggers.Common.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Repositories.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserSessionModel> LoggedUser { get; set; }
        public DbSet<AdminActionLogModel> AdminActionLog { get; set; }
        public DbSet<HistUserLoginModel> HistUserLogin { get; set; }
        public DbSet<HistAplicationUserModel> HistAplicationUser { get; set; }
        public DbSet<TestDataModel> TestData { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ///
            // TABLE: TB_USUAR_SESSION
            // MODEL: UserSessionModel
            ///
            builder.Entity<UserSessionModel>()
                   .AfterInsert(trigger =>
                       trigger.Action(action =>
                            action.Insert<HistUserLoginModel>(userSessionModel => new HistUserLoginModel
                            {
                                IdSessao = userSessionModel.New.IdSessao,
                                FkIdUsuario = userSessionModel.New.FkIdUsuario,
                                DataInclusao = userSessionModel.New.DataInclusao,
                                UF = userSessionModel.New.UF,
                                Provedora = userSessionModel.New.Provedora,
                                IP = userSessionModel.New.IP,
                                Platform = userSessionModel.New.Platform,
                                UserAgent = userSessionModel.New.UserAgent,
                                DbOperationType = (char)DbOperationTypeEnum.INCLUSAO
                            })));

            ///
            // TABLE: AspNetUsers
            // MODEL: ApplicationUser
            ///
            builder.Entity<ApplicationUser>()
                   .AfterInsert(trigger =>
                       trigger.Action(action =>
                            action.Insert<HistAplicationUserModel>(applicationUser => new HistAplicationUserModel
                            {
                                Id = applicationUser.New.Id,
                                UserName = applicationUser.New.UserName,
                                NormalizedUserName = applicationUser.New.NormalizedUserName,
                                Email = applicationUser.New.Email,
                                NormalizedEmail = applicationUser.New.NormalizedEmail,
                                PasswordHash = applicationUser.New.PasswordHash,
                                PhoneNumber = applicationUser.New.PhoneNumber,
                                Name = applicationUser.New.Name,
                                DbOperationType = (char)DbOperationTypeEnum.INCLUSAO,
                                DataInclusao = DateTime.Now
                            })))
                   .AfterUpdate(trigger =>
                        trigger.Action(action =>
                            action.Insert<HistAplicationUserModel>(applicationUser => new HistAplicationUserModel
                            {
                                Id = applicationUser.New.Id,
                                UserName = applicationUser.New.UserName,
                                NormalizedUserName = applicationUser.New.NormalizedUserName,
                                Email = applicationUser.New.Email,
                                NormalizedEmail = applicationUser.New.NormalizedEmail,
                                PasswordHash = applicationUser.New.PasswordHash,
                                PhoneNumber = applicationUser.New.PhoneNumber,
                                Name = applicationUser.New.Name,
                                DbOperationType = (char)DbOperationTypeEnum.ALTERACAO,
                                DataInclusao = DateTime.Now
                            })));


            ///
            // TABLE: TB_LOG_ADMIN_ACTION
            // MODEL: AdminActionLogModel
            ///
            builder.HasSequence<long>("admin_action_sequence")
                   .StartsAt(1)
                   .IncrementsBy(1);

            builder.Entity<AdminActionLogModel>()
                   .Property(e => e.IdAction)
                   .HasDefaultValueSql("nextval('\"admin_action_sequence\"')");


            ///
            // TABLE: TB_TEST
            // MODEL: TestDataModel
            ///
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
