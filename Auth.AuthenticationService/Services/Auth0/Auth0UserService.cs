using System;
using System.Threading.Tasks;
using Auth.AuthenticationService.Services.Common;
using Auth.Models.Dtos;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Auth.AuthenticationService.Services.Auth0;

public class Auth0UserService : IUserManager
{
    private readonly IAuthenticationApiClient _authenticationApiClient;
    private readonly ILogger<Auth0UserService> _logger;
    private IManagementService _managementService;

    public Auth0UserService(IManagementService managementService, ILogger<Auth0UserService> logger, IAuthenticationApiClient client = null)
    {
        // Initialize Auth0 client with your Auth0 credentials
        _authenticationApiClient = client ?? new AuthenticationApiClient(Globals.Auth0Domain);
        _managementService = managementService;
        _logger = logger;
    }

    /// <summary>
    /// Using the management api we are updating user profile and setting the New Password
    /// </summary>
    /// <param name="idToken"></param>
    /// <param name="changePassword"></param>
    /// <returns></returns>
    public async Task<string> ChangePassword(string idToken, ChangePasswordDto changePassword)
    {
        //Same Password Check
        if (changePassword.Password == changePassword.NewPassword)
        {
            _logger.LogError("Tried to update password with same Password");
            throw new ArgumentException("same_password");
        }

        //TODO: refactor and Validate via signin
        // try
        // {
        //     await Auth0Service.SignInAsync(changePassword);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Couldn't verify user to change password");
        //     throw new ArgumentException("Couldn't verify user to change password");
        // }

        //If there is no exception then we can update the user.

        // Create user update request
        var updateRequest = new UserUpdateRequest
        {
            Password = changePassword.NewPassword
        };

        try
        {
            var user = Utilities.GetUserFormIdToken(idToken);
            if (user.FullName != changePassword.Email)
            {
                _logger.LogError("Invalid change password request");
                throw new ArgumentException("email_mismatch");
            }

            //Get the management Api client instance.
            var _managementApiClient = await _managementService.GetManagmentApiClient();
            // Update user password
            var result =
                await _managementApiClient.Users.UpdateAsync(user.UserId, updateRequest); // Process result as needed
            _logger.LogInformation($"User {user.NickName} Updated successfull");

            return $"Password for {user.NickName} Updated successfull";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Using the management api we are updating user profile and setting the new Information
    /// </summary>
    /// <param name="idToken"></param>
    /// <param name="userInfoDto"></param>
    /// <returns></returns>
    public async Task<string> UpdateUser(string idToken, UserInfoDto userInfoDto)
    {
        UserInfo user;

        // Create user update request
        var updateRequest = new UserUpdateRequest
        {
            NickName = userInfoDto.NickName
        };

        try
        {
            var userId = Utilities.GetUserIdFromToken(idToken);
            //Get the management Api client instance.
            var _managementApiClient = await _managementService.GetManagmentApiClient();
            // Update user password
            var result = await _managementApiClient.Users.UpdateAsync(userId, updateRequest);
            _logger.LogInformation($"User {userId} Updated successfull");
            return $"User {userId} Updated successfull";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Fetch User details from Auth0.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<UserInfoDto> User(string token)
    {
        // Signin user using Auth0 API
        try
        {
            var user = await _authenticationApiClient.GetUserInfoAsync(token);
            _logger.LogInformation($"User {user.UserId} Logged In.");
            //return user as UserInfoDto;
            return JsonConvert.DeserializeObject<UserInfoDto>(JsonConvert.SerializeObject(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }
    }
}