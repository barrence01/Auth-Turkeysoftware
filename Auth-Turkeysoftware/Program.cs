using Auth_Turkeysoftware.Repositories.Context;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.HttpOverrides;
using Auth_Turkeysoftware.Models.Identity;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Auth_Turkeysoftware.Extensions;

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
    // Controllers e Swagger
    ////
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiDocument();

    ////
    // Serviços criados
    ////
    builder.Services.AddCustomServices()
                    .AddCustomSingletons(builder.Configuration)
                    .AddCustomTransients(builder.Configuration)
                    .AddCustomHandlers(builder.Configuration);

    ////
    // Entity Framework
    // Acesso ao banco de dados
    ////
    builder.Services.AddAppDbContexts(builder.Configuration);

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

    ////
    // Adição de autenticação e autorização via JWE TOKEN
    ////
    builder.Services.AddJwtAuthentication(builder.Configuration);
    

    // Configura o MVC para exigir autenticação globalmente
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