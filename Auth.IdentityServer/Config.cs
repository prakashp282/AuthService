using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> { "role" }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new("read:messages")
        };


    //Using the same client key as api key to encode.
    public static IEnumerable<ApiResource> ApiResources =>
        new[]
        {
            new ApiResource("Messages")
            {
                Scopes = new List<string> { "read:messages", },
                ApiSecrets = new List<Secret> { new Secret(Environment.GetEnvironmentVariable("SECRET_KEY").Sha256()) },
                UserClaims = new List<string> { "role" }
            }
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new()
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",
                AllowedGrantTypes = new[]
                    { GrantType.ClientCredentials, GrantType.Hybrid, "MfaVerification", "refresh_token" },
                ClientSecrets = { new Secret("ClientSecret1".Sha256()) },
                AllowedScopes = { "openid", "profile", "offline_access", "read:messages" },
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 86400, // 1 day
                UpdateAccessTokenClaimsOnRefresh = true
            },
            // interactive client using code flow + pkce
            new()
            {
                ClientId = Environment.GetEnvironmentVariable("CLIENT_ID"),
                ClientName = Environment.GetEnvironmentVariable("CLIENT_NAME"),
                ClientSecrets = { new Secret(Environment.GetEnvironmentVariable("SECRET_KEY").Sha256()) },
                //ClientSecrets = { new Secret("raAAGQOkU/NU5WC2OGf0LFof0FMePQgEMVBAakqLgOs=")},
                //// where to redirect to after login
                RedirectUris = { Environment.GetEnvironmentVariable("SERVER_URL") + "/swagger/oauth2-redirect.html" },
                //// where to redirect to after logout
                PostLogoutRedirectUris = { Environment.GetEnvironmentVariable("SERVER_URL") },
                AllowedCorsOrigins = { Environment.GetEnvironmentVariable("SERVER_URL") },
                AllowedScopes = new List<string>
                {
                    "openid", "profile", "offline_access", "read:messages"
                },
                RequireClientSecret = true,
                AllowedGrantTypes = new[]
                {
                    GrantType.AuthorizationCode, GrantType.ResourceOwnerPassword, "MfaVerification", "refresh_token"
                },
                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 86400, // 1 day
                UpdateAccessTokenClaimsOnRefresh = true
                //RequirePkce = true,
                //RequireConsent = true,
                //AllowPlainTextPkce = false
            }
        };

    public static IEnumerable<IdentityRole> Roles =>
        new IdentityRole[]
        {
            new IdentityRole("Admin"),
            new IdentityRole("Default"),
            new IdentityRole("Special")
        };
}