using Auth_Turkeysoftware.Domain.Models.Identity;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.DbContext;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities.Identity;
using Auth_Turkeysoftware.Shared.Enums;
using Auth_Turkeysoftware.Shared.Extensions;
using Auth_Turkeysoftware.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;
using Serilog.Events;


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

    ////
    // Load environment variables from a .env file
    ////
    ConfigUtil.LoadEnvironmentVariablesFromEnvFile(builder);

    ////
    // Add serilog as the default logger provider
    ////
    builder.Host.UseSerilog();

    ////
    // Controllers e Swagger
    ////
    builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiDocument();

    ////
    // Load services
    ////
    builder.Services.AddCustomScoped()
                    .AddCustomSingletons(builder.Configuration)
                    .AddCustomTransients(builder.Configuration)
                    .AddCustomHandlers(builder.Configuration);

    ////
    // Entity Framework
    // Sets ups database access
    ////
    builder.Services.AddDbContexts(builder.Configuration);

    ////
    // Microsoft Identity
    // Authorization and authentication
    ////
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddErrorDescriber<CustomIdentityErrorDescriber>()
                    .AddDefaultTokenProviders()
                    .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(TokenOptions.DefaultEmailProvider);


    // Sets the requirements for a new account
    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.AllowedForNewUsers = true;
    });

    ////
    // Add authorization and authentication using JWE TOKEN
    ////
    builder.Services.AddJwtAuthentication(builder.Configuration);
    

    // Sets authentication required to all routes by default
    builder.Services.AddControllers(config =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        config.Filters.Add(new AuthorizeFilter(policy));
    });


    ////
    // CORS
    // Host access control
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

    // Automatizally execute migrations on start up
#if !DEBUG
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