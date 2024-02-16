using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Resources;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();
builder.Configuration.AddEnvironmentVariables();
bool useIdentityServer = Convert.ToBoolean(Environment.GetEnvironmentVariable("USE_IDENTITY_SERVER"));
string domain = builder.Configuration.GetValue<string>("AUTH0_DOMAIN");
string audience =  builder.Configuration.GetValue<string>( useIdentityServer? "IDENTITY_SERVER_AUDIENCE": "AUTH0_AUDIENCE");
string issuer = useIdentityServer? builder.Configuration.GetValue<string>("IDENTITY_SERVER_HOST") : $"https://{domain}/"; 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

//Setup Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer("Bearer", options =>
{
    options.Authority = issuer;
    options.Audience = audience;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

//Setup Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("read:messages",
        policy => policy.Requirements.Add(new HasScopeRequirement("read:messages", issuer)));
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseRouting();
app.UseCors();
app.UseHttpsRedirection();
//Just to add some logs on request.
app.UseCustomLoggingMiddleware();

//Authentication and authorization
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.Run();