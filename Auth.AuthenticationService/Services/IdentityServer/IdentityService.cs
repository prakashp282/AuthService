using System;
using System.Threading.Tasks;
using Auth.Models.Common;
using Auth.Models.Dtos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Auth.AuthenticationService.Services.IdentityServer;

public class IdentityService : IAuthenticationProvider
{
    private readonly IRestClient _client;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(ILogger<IdentityService> logger, IRestClient client = null)
     {
        _client = client ?? new RestClient(Globals.IdentityServerHost);
        _logger = logger;
    }

    public async Task<AccessTokenDto> SignUpWithMFAAsync(SignUpDto signUp)
    {
        _logger.LogInformation("Sign up with Identity Server");
        var request = new RestRequest(SignInUrl, Method.Post);
        request.AddJsonBody(signUp);
        var response = await _client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            _logger.LogError("Error while signUp ", response.Content);
            // Handle registration error
            throw new Exception(response.Content);
        }

        _logger.LogInformation("Signed Up successufully");
        return JsonConvert.DeserializeObject<AccessTokenDto>(response.Content);
    }

    public async Task<AccessTokenDto> SignInWithMFAAsync(SignInDto signIn)
    {
        _logger.LogInformation("Signin with Identity Server");
        try
        {
            var request = new RestRequest(GetTokenUrl, Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", GrantType.ResourceOwnerPassword);
            request.AddParameter("client_id", Globals.IdentityServerClientId);
            request.AddParameter("client_secret", Globals.IdentityServerClientSecret);
            request.AddParameter("username", signIn.Email);
            request.AddParameter("password", signIn.Password);
            request.AddParameter("scope", Globals.RequestScope);

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Signed In successufully");
                return JsonConvert.DeserializeObject<AccessTokenDto>(response.Content);
            }
            else if (response.Content != null &&
                     JsonConvert.DeserializeObject<JObject>(response.Content)?["error_description"]?.Value<string>() ==
                     "mfa_required")
            {
                return new AccessTokenDto()
                {
                    NeedsMFA = true,
                    MFAToken = response.Cookies["Identity.TwoFactorUserId"]?.ToString()
                };
            }
            else
            {
                _logger.LogError("error while signIn", response?.Content ?? response.ErrorMessage);
                throw new ArgumentException(response?.Content ?? response?.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error SiginIN with Auth.IdentityServer: " + ex.Message);
        }
    }

    public async Task<AccessTokenDto> VerifyMfaChallengeAsync(string mfaToken, string challengeId,
        VerifyChallengeDto verifyChallenge)
    {
        try
        {
            var request = new RestRequest(GetTokenUrl, Method.Post);
            //var request = new RestRequest("/VerifyCode", Method.Post);

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Cookie", mfaToken);
            request.AddParameter("grant_type", GrantType.MfaVerification);
            request.AddParameter("client_id", Globals.IdentityServerClientId);
            request.AddParameter("client_secret", Globals.IdentityServerClientSecret);
            request.AddParameter("scope", Globals.RequestScope);
            request.AddParameter("code", verifyChallenge.Otp);

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Signed In successufully");
                return JsonConvert.DeserializeObject<AccessTokenDto>(response.Content);
            }
            else
            {
                _logger.LogError("error while signIn", response.Content);
                throw new ArgumentException(response.Content);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error MFA Verification with Auth.IdentityServer: " + ex.Message);
        }
    }

    public async Task<string> ResetPasswordAsync(string email)
    {
        _logger.LogInformation("reset password with Identity Server");
        var request = new RestRequest(ResetPasswordUrl, Method.Post);
        // request.AddParameter("email", email);
        request.AddBody(new { email });
        var response = await _client.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            _logger.LogError("Error while Reset password ", response.Content);
            // Handle registration error
            throw new Exception("Error Reset Password with Auth.IdentityServer: " + response.Content);
        }

        return "Password Reset mail Sent";
    }


    public async Task<string> SignOutAsync()
    {
        //TODO: update this
        try
        {
            var request = new RestRequest(LogoutUrl, Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("id_token_hint", "YOUR_ID_TOKEN"); // Replace with the actual ID token
            request.AddParameter("post_logout_redirect_uri", "https://your-app-url/logout");

            var response = await _client.ExecuteAsync(request);
            //TODO: Need to return the redirect logout url here
            return String.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Logging out of Auth.IdentityServer");
            throw;
        }
    }

    public async Task<AccessTokenDto> GetValidAccessToken(string refreshToken)
    {
        try
        {
            // If invalid, attempt to refresh the token
            var request = new RestRequest(GetTokenUrl, Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", GrantType.RefreshToken);
            request.AddParameter("client_id", Globals.IdentityServerClientId);
            request.AddParameter("client_secret", Globals.IdentityServerClientSecret);
            request.AddParameter("refresh_token", refreshToken);

            var response = await _client.ExecuteAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("error while Refresh", response.Content);
                throw new ArgumentException(response.Content);
            }

            _logger.LogInformation("Refreshed successufully");
            return JsonConvert.DeserializeObject<AccessTokenDto>(response.Content);
        }
        catch (Exception ex)
        {
            // Handle errors, such as invalid refresh token or network issues
            // Log the error and potentially trigger user re-authentication
            _logger.LogError(message: "Error while getting Refresh token " + ex.Message);
            throw;
        }
    }

    #region ExposedEndPoints

    private const string GetTokenUrl = "/connect/token";
    private const string LogoutUrl = "/connect/logout";
    private const string SignInUrl = "/Register";
    private const string ResetPasswordUrl = "/ResetPassword";

    #endregion
}