using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Repositories.Context;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog.Events;
using Microsoft.AspNetCore.HttpOverrides;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Services.ExternalServices;
using Auth_Turkeysoftware.Services.MailService;
using Auth_Turkeysoftware.Models.Identity;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Controllers.Handlers;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Auth_Turkeysoftware.Test.Repositories;
using Auth_Turkeysoftware.Configurations.Models;
using Auth_Turkeysoftware.Configurations.Services;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Auth_Turkeysoftware.Services.DistributedCacheService;

// Logging provider
Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            #if !DEBUG
                .MinimumLevel.Warning()
            #else
                .MinimumLevel.Information()
            #endif
            .WriteTo.Async(a => a.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u4}] [{SourceContext}] :: {Message:l}{NewLine}{Exception}"))
            #if !DEBUG
            .WriteTo.Async(a => a.File("logs/auth_turkeysoftware-log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7,
                            outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u4}] [{SourceContext}] :: {Message:l}{NewLine}{Exception}"))
            #endif
            .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    // Adicionar log do serilog como padrão
    builder.Host.UseSerilog();

    ////
    // Serviços criados
    ////
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiDocument();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddScoped<IPostgresCacheRepository, PostgresCacheRepository>();
    builder.Services.AddScoped<IDistributedCacheService, DistributedCacheService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ITwoFactorRepository, TwoFactorRepository>();
    builder.Services.AddScoped<IUserSessionService, UserSessionService>();
    builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
    builder.Services.AddScoped<IExternalApiService, ExternalApiService>();
    builder.Services.AddScoped<IAdministrationService, AdministrationService>();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IAccountRecoveryService, AccountRecoveryService>();
    builder.Services.AddScoped<IRegisterUserService, RegisterUserService>();
    builder.Services.AddScoped<ICommunicationService, CommunicationService>();
    builder.Services.AddScoped<ITestDataRepository, TestDataRepository>();

    // Singleton Service
    JwtSettingsSingleton jwtSettingsSingleton = new(new JwtSettings
    {
        EncryptionKey = GetRequiredEnvVar("JWT_ENCRYPTION_KEY"),
        LoginSecretKey = GetRequiredEnvVar("JWT_LOGIN_SECRET"),
        AccessSecretKey = GetRequiredEnvVar("JWT_ACCESS_SECRET"),
        RefreshSecretKey = GetRequiredEnvVar("JWT_REFRESH_SECRET"),
        Issuer = GetRequiredEnvVar("JWT_ISSUER"),
        Audience = GetRequiredEnvVar("JWT_AUDIENCE"),
        Domain = GetRequiredEnvVar("JWT_DOMAIN"),
        AccessTokenValidityInMinutes = GetEnvVarAsInt("JWT_ACCESS_TOKEN_MINUTES"),
        RefreshTokenValidityInMinutes = GetEnvVarAsInt("JWT_REFRESH_TOKEN_MINUTES"),
        RefreshTokenPath = GetRequiredEnvVar("JWT_REFRESH_TOKEN_PATH") ?? "/api/Auth/refresh-token",
        AccessTokenPath = GetRequiredEnvVar("JWT_ACCESS_TOKEN_PATH") ?? "/"
    });
    builder.Services.AddSingleton<JwtSettingsSingleton>(jwtSettingsSingleton);
    builder.Services.AddSingleton<HttpClientSingleton>();
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddSingleton<EmailSettingsSingleton>();
    builder.Services.Configure<EmailTokenProviderSettings>(builder.Configuration.GetSection("EmailTokenProviderSettings"));
    builder.Services.AddSingleton<EmailTokenProviderSingleton>();

    // Mail Service
    builder.Services.AddTransient<IEmailService, EmailService>();

    // Filters
    builder.Services.AddScoped<LoginFilter>();
    builder.Services.AddScoped<AdminActionLoggingFilterAsync>();

    // Global Exeption Handler
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    ////
    // Entity Framework
    // Acesso ao banco de dados
    ////
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContextPool<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure();
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: new List<string>
            {
                "57014", // Query timeout (PostgreSQL)
                "53300", // Too many connections
                "53400", // Configuration limit exceeded
                "08000", // Connection exception
                "08003", // Connection does not exist
                "08006", // Connection failure
                "08001", // SQL-client unable to establish SQL-connection
                "08004", // SQL-server rejected establishment of SQL-connection
                "08007", // Transaction resolution unknown
                "40P01"  // Deadlock detected
            });
        });
        options.UsePostgreSqlTriggers();
    });

    ////
    // Acesso separado para o dbcontext do distributedCache para que atualizações no cache
    // não afetem transações em progresso na mesma requisição
    ////
    var connectionStringDistributedCache = builder.Configuration.GetConnectionString("CacheDbConnection");
    builder.Services.AddDbContextPool<CacheDbContext>(options =>
    {
        options.UseNpgsql(connectionStringDistributedCache, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure();
        });
    });

    ////
    // Microsoft Identity
    // Autenticação e autorização
    ////
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddErrorDescriber<CustomIdentityErrorDescriber>()
                    .AddDefaultTokenProviders()
                    .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(TokenOptions.DefaultEmailProvider);


    // Configurar requisitos para login de usuário
    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.AllowedForNewUsers = true;
    });


    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,

            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:AccessSecretKey")
                                                                               ?? throw new InvalidOperationException("AccessSecretKey is missing in configuration."))),
            TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:EncryptionKey")
                                                                                 ?? throw new InvalidOperationException("EncryptionKey is missing in configuration.")))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["TurkeySoftware-AccessToken"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                    context.Response.Cookies.Delete("TurkeySoftware-AccessToken", new CookieOptions
                                                    {
                                                        HttpOnly = true,
                                                        Secure = true,
                                                        IsEssential = true,
                                                        SameSite = SameSiteMode.None,
                                                        Domain = builder.Configuration.GetSection("JwtSettings:Domain").Value,
                                                        Path = "/"
                                                    });
                return Task.CompletedTask;
            }
        };
    });

    // Configurar MVC para exigir autenticação globalmente
    builder.Services.AddControllers(config =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        config.Filters.Add(new AuthorizeFilter(policy));
    });


    ////
    // CORS
    // Controle de acesso ao host
    ////
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AcessoElevado", policy =>
                policy.RequireRole(UserRolesEnum.Master.ToString(), UserRolesEnum.Admin.ToString()));

        options.AddPolicy("DenyGuests", policy =>
        policy.RequireAssertion(context =>
            !context.User.HasClaim(c => c.Type == nameof(UserRolesEnum.Guest))));
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowLocalhost",
                          policy =>
                          {
                              policy.WithOrigins(@"http://localhost:3000", @"http://localhost:7157", @"http://localhost:3001", @"http://app.localhost:3001")
                              .AllowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                          });
        options.AddPolicy(name: "Production",
                          policy =>
                          {
                              policy.WithOrigins(@"http://meudominio.com.br")
                              .AllowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                          });
    });

    var app = builder.Build();

    app.UseExceptionHandler(o => {});

    #if !DEBUG
        app.UseCors("Production");
    #else
        app.UseCors("AllowLocalhost");
    #endif

    app.UseSerilogRequestLogging();

    // Obter IP caso seja redirecionamento
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
    });

#if !DEBUG
    // Verifica se o banco de dados está em conformidade com as migrações, caso não esteja, executará o migrations para sincronizar a base.
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }
#endif

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseOpenApi(); // https://localhost:7157/swagger/index.html
        app.UseSwaggerUi(); 
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    // Log the URL and Port Information
    var url = builder.Configuration["ASPNETCORE_URLS"];
    var port = Environment.GetEnvironmentVariable("ASPNETCORE_PORT");

    Log.Warning("Application starting up...");
    Log.Warning("Server URL: {URL}, Port: {Port}", url, port);
    Log.Warning("To access swagger add path '/swagger/index.html'.");
    Log.Warning("Example: https://localhost:7157/swagger/index.html");

   await app.RunAsync();
}
catch (HostAbortedException) {
    Log.Information("Aplicação foi finalizada.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
static string GetRequiredEnvVar(string name) => Environment.GetEnvironmentVariable(name)
                                                ?? throw new InvalidOperationException($"Variável de ambiente obrigatória '{name}' não existe.");

static int GetEnvVarAsInt(string name) => int.TryParse(Environment.GetEnvironmentVariable(name), out int result) ? result
                                          : throw new InvalidOperationException($"Variável de ambiente obrigatória '{name}' não existe.");