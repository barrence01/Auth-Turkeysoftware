using Auth_Turkeysoftware.Models.DataBaseModels;
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
using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Services.MailService;
using MySqlConnector;
using Auth_Turkeysoftware.Models.Identity;

// Logging provider
Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .MinimumLevel.Information()
            .WriteTo.Async(a => a.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u4}] {Message:l}{NewLine}{Exception}"))
            #if !DEBUG
            .WriteTo.Async(a => a.File("logs/auth_turkeysoftware-log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7,
                            outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u4}] {Message:l}{NewLine}{Exception}"))
            #endif
            .CreateLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddSerilog();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiDocument();
    builder.Services.AddScoped<ILoggedUserService, LoggedUserService>();
    builder.Services.AddScoped<ILoggedUserRepository, LoggedUserRepository>();
    builder.Services.AddScoped<IExternalApiService, ExternalApiService>();

    // Mail Service
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddTransient<IEmailService, EmailService>();

    //Filters
    builder.Services.AddScoped<LoginFilter>();

    // Entity Framework
    var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");

    // Uso de AddDbContextPool para reutilizar as mesmas instancias de DbContext
    // Devido a isso, não será possível usar o OnConfiguring do DbContext
    builder.Services.AddDbContextPool<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    // Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddErrorDescriber<CustomIdentityErrorDescriber>()
                    .AddDefaultTokenProviders();

    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.AllowedForNewUsers = true;
    });


    // Authentication
    //var jwtAuthorites = builder.Configuration.GetSection("JwtBearerToken:JwtAuthorities").GetChildren().Select(c => c.GetValue<string>("Issuer")).ToList();
    //Console.WriteLine(string.Concat("Valid Issuers: ", string.Join(",", jwtAuthorites)));
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    // Jwt Bearer
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,


            ValidIssuer = builder.Configuration["JwtBearerToken:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearerToken:AccessSecretKey"]))
            //ValidIssuers = jwtAuthorites,
            //ValidAudience = builder.Configuration["JwtBearerToken:Audience"],
            //IssuerSigningKeys = builder.Configuration["JwtBearerToken:SignKey"],
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["TurkeySoftware-AccessToken"];
                return Task.CompletedTask;
            },
        };
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

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
        app.UseOpenApi(); /// https://localhost:7157/swagger/index.html
        app.UseSwaggerUi(); //Add pacote swagger from NSwag.AspNetCore
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
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
    Log.CloseAndFlush();
}