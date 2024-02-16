using System;
using Auth.AuthenticationService.Middlewares;
using Auth.AuthenticationService.Services;
using Auth.AuthenticationService.Services.Auth0;
using Auth.AuthenticationService.Services.IdentityServer;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//builder.Configuration.Sources.Clear();
DotEnv.Load();
builder.Configuration.AddEnvironmentVariables();

bool useIdentityServer = Convert.ToBoolean(Environment.GetEnvironmentVariable("USE_IDENTITY_SERVER"));

builder.Services.AddLogging(); // Adds the default logging providers

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetValue<string>("CLIENT_ORIGIN_URL"))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string domain = builder.Configuration.GetValue<string>("AUTH0_DOMAIN");
string audience =
    builder.Configuration.GetValue<string>(useIdentityServer ? "IDENTITY_SERVER_AUDIENCE" : "AUTH0_AUDIENCE");
string clientId =
    builder.Configuration.GetValue<string>(useIdentityServer ? "IDENTITY_SERVER_CLIENT_ID" : "AUTH0_CLIENT_ID");
string issuer = useIdentityServer
    ? builder.Configuration.GetValue<string>("IDENTITY_SERVER_HOST")
    : $"https://{domain}/";

//Setup Authentication.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
        //To retrive auth token from the cookies
        options.Cookie.Name = "token"
    )
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = issuer;
        options.Audience = audience;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateLifetime = false, // using middleware we will refresh token.
            ClockSkew = TimeSpan.Zero //JUST TO TEST TOKEN EXP in by default the .net tolerance time is 5 mins
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                //When we recieve request get the token from cookies.
                string accessToken = context.Request.Cookies["token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }

                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });


//Setup Authorization
builder.Services.AddAuthorization();

// Add services to the container.
if (useIdentityServer)
{
    builder.Services.AddScoped<IAuthenticationProvider, IdentityService>();
    builder.Services.AddScoped<IRoleManager, RoleService>();
    builder.Services.AddScoped<IUserManager, IdentityUserService>();
}
else
{
    builder.Services.AddScoped<IAuthenticationProvider, Auth0Service>();
    builder.Services.AddScoped<IUserManager, Auth0UserService>();
    builder.Services.AddScoped<IManagementService, ManagementService>();
    builder.Services.AddScoped<IMFAService, MfaService>();
}

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseErrorHandler();
app.UseSecureHeaders();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAccessTokenValidation();
app.UseAuthorization();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();