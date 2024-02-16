using System;

namespace Auth.AuthenticationService;

public static class Globals
{
    public static string Auth0Domain { get; private set; } = Environment.GetEnvironmentVariable("AUTH0_DOMAIN");
    public static string Auth0ClientId { get; private set; } = Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID");

    public static string Auth0ClientSecret { get; private set; } =
        Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET");

    public static string Auth0Connection { get; private set; } =
        Environment.GetEnvironmentVariable("AUTH0_DB_CONNECTION");

    public static string Auth0Audience { get; private set; } = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE");

    //Scopes to ask when requesting for token.
    // Scope = offline_access - for refresh token, enroll - for enrollement mfa, profile - for Id Token, openid - for Sub Id
    public static string RequestScope { get; private set; } =
        "openid profile offline_access read:messages"; //enroll removed

    public static string IdentityServerHost { get; private set; } =
        Environment.GetEnvironmentVariable("IDENTITY_SERVER_HOST");

    public static string IdentityServerClientId { get; private set; } =
        Environment.GetEnvironmentVariable("IDENTITY_SERVER_CLIENT_ID");

    public static string IdentityServerClientSecret { get; private set; } =
        Environment.GetEnvironmentVariable(
            "IDENTITY_SERVER_CLIENT_SECRET"); //new Secret(Environment.GetEnvironmentVariable("IDENTITY_SERVER_CLIENT_SECRET").Sha256()) ;
}