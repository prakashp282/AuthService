using System;
using System.Threading.Tasks;
using Auth.AuthenticationService.Services.Common;
using Auth.Models.Dtos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Auth.AuthenticationService.Services.IdentityServer;

public class IdentityUserService : IUserManager
{
    private readonly IRestClient _client;
    private readonly ILogger<IdentityUserService> _logger;

    public IdentityUserService(ILogger<IdentityUserService> logger, IRestClient client = null)
    {
        _client = client ?? new RestClient(Globals.IdentityServerHost);
        _logger = logger;
    }

    public async Task<UserInfoDto> User(string token)
    {
        try
        {
            var request = new RestRequest(GetUserUrl);
            request.AddHeader("Authorization", "Bearer" + token);

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<UserInfoDto>(response.Content);
            }
            else
            {
                _logger.LogError("error while getting User", response.Content);
                throw new ArgumentException(response.Content);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Logging out of Auth.IdentityServer");
            throw;
        }
    }

    public async Task<string> ChangePassword(string idToken, ChangePasswordDto changePassword)
    {
        //TODO : verify from token the email is the same.

        _logger.LogInformation("Change password with Identity Server");
        var request = new RestRequest(ChangePasswordUrl, Method.Post);
        request.AddJsonBody(changePassword);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            _logger.LogInformation("Password changed successufully");
            //Ideally should logout user here  OR return new access and id token.
            //return JsonConvert.DeserializeObject<AccessToken>(response.Content);
            return "Password Updated Successfully";
        }
        else
        {
            _logger.LogError("Error while Change password ", response.ErrorMessage);
            // Handle registration error
            throw new Exception("Error Change Password with Auth.IdentityServer: " + response.ErrorMessage);
        }
    }

    public async Task<string> UpdateUser(string idToken, UserInfoDto userInfoDto)
    {
        try
        {
            var userId = Utilities.GetUserIdFromToken(idToken);
            // Update user password
            var request = new RestRequest(UpdateUserUrl, Method.Post);
            request.AddJsonBody(new
            {
                Id = userId,
                PhoneNumber = userInfoDto.PhoneNumber
            });
            var response = await _client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                _logger.LogError("Error while update ", response.ErrorMessage);
                // Handle registration error
                throw new Exception("Error Update with Auth.IdentityServer: " + response.ErrorMessage);
            }

            _logger.LogInformation($"User {userId} Updated successfull");
            return $"User {userId} Updated successfull";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }
    }

    #region ExposedEndPoints

    private const string GetUserUrl = "/connect/userinfo";
    private const string ChangePasswordUrl = "/ChangePassword";
    private const string UpdateUserUrl = "/UpdateUser";

    #endregion
}