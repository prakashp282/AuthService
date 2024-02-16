namespace IdentityServer.Services;

public class SMSOptions
{
    public string? SMSAccountIdentification { get; set; } = Environment.GetEnvironmentVariable("ACCOUNT_SID");
    public string? SMSAccountPassword { get; set; } = Environment.GetEnvironmentVariable("AUTH_TOKEN");
    public string? SMSAccountFrom { get; set; } = Environment.GetEnvironmentVariable("FROM_NUMBER");
}