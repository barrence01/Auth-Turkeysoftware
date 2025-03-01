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

var builder = WebApplication.CreateBuilder(args);

// Logging provider
Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Async(a => a.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u4}] {Message:l}{NewLine}{Exception}"))
            //.WriteTo.Async(a => a.File("logs/auth_turkeysoftware-log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7,
            //                outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u4}] {Message:l}{NewLine}{Exception}"))
            .CreateLogger();


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();
builder.Services.AddScoped<ILoggedUserService, LoggedUserService>();
builder.Services.AddScoped<ILoggedUserRepository, LoggedUserRepository>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

// Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

// Authentication
var jwtAuthorites = builder.Configuration.GetSection("JwtBearerToken:JwtAuthorities").GetChildren().Select(c => c.GetValue<string>("Issuer")).ToList();
Console.WriteLine(string.Concat("Valid Issuers: ", string.Join(",", jwtAuthorites)));
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
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,

        ValidIssuers = jwtAuthorites,
        //ValidIssuer = builder.Configuration["JwtBearerToken:Issuer"],
        ValidAudience = builder.Configuration["JwtBearerToken:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearerToken:AccessSecretKey"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["AccessToken"];
            return Task.CompletedTask;
        },
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(); /// https://localhost:7157/swagger/index.html
    app.UseSwaggerUi(); //Add swagger from NSwag.AspNetCore
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();