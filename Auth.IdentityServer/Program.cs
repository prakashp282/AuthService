using dotenv.net;
using IdentityServer;
using IdentityServer.Data;
using IdentityServer.Middlewares;
using IdentityServer.Models;
using IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SendGrid.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

//builder.Configuration.Sources.Clear();
DotEnv.Load();
builder.Configuration.AddEnvironmentVariables();

//Get the current assembly name 
var assembly = typeof(Program).Assembly.GetName().Name;
var defaultConnString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");

// Add services to the container.
builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(defaultConnString, new MySqlServerVersion(new Version(8, 0)),
        opt => opt.MigrationsAssembly(assembly).EnableRetryOnFailure()));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        //options.SignIn.RequireConfirmedEmail = true;
        options.User.RequireUniqueEmail = true;
        options.Tokens.EmailConfirmationTokenProvider = "Email";
        options.Tokens.ChangeEmailTokenProvider = "Email";
        options.Tokens.PasswordResetTokenProvider = "Email";
        //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        //options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddUserManager<UserManager<ApplicationUser>>()
    .AddSignInManager<SignInManager<ApplicationUser>>();

//Using SQL server to store tables
builder.Services.AddIdentityServer()
    .AddAspNetIdentity<ApplicationUser>()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseMySql(defaultConnString, new MySqlServerVersion(new Version(8, 0)),
                opt => opt.MigrationsAssembly(assembly).EnableRetryOnFailure());
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseMySql(defaultConnString, new MySqlServerVersion(new Version(8, 0)),
                opt => opt.MigrationsAssembly(assembly).EnableRetryOnFailure());
    })
    .AddDeveloperSigningCredential(persistKey: false)
    .AddResourceOwnerValidator<UserValidator>()
    .AddExtensionGrantValidator<AuthenticationGrant>();

builder.Services.AddScoped<IAccountService, AccountService>();
//Set SMS
builder.Services.AddTransient<ISmsSender, MessageSender>();
builder.Services.Configure<SMSOptions>(builder.Configuration);
//Set Email
builder.Services.AddSendGrid(options => { options.ApiKey = Environment.GetEnvironmentVariable("EMAIL_API_KEY"); });
builder.Services.AddScoped<IEmailSender, EmailSender>();
//IF we want to add additional custom User Claims to the token.
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
    AdditionalUserClaimsPrincipalFactory>();

var app = builder.Build();


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseErrorHandler();
//The middleware that would expose all the identity server enabled endpoints.
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

if (args.Contains("/seed"))
{
// this seeding is only for the template to bootstrap the DB and users.
// in production you will likely want a different approach.
    SeedData.EnsureSeedData(defaultConnString);
}

SeedData.SeedRoles(app);

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();