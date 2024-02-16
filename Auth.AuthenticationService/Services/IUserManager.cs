using System.Threading.Tasks;
using Auth.Models.Dtos;

namespace Auth.AuthenticationService.Services;

public interface IUserManager
{
    /// <summary>
    /// Fetch User details from Auth0.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<UserInfoDto> User(string token);

    /// <summary>
    /// Using the management api we are updating user profile and setting the new Information
    /// </summary>
    /// <param name="idToken"></param>
    /// <param name="userInfoDto"></param>
    /// <returns></returns>
    Task<string> UpdateUser(string idToken, UserInfoDto userInfoDto);

    /// <summary>
    /// Using the management api we are updating user profile and setting the New Password
    /// </summary>
    /// <param name="idToken"></param>
    /// <param name="changePassword"></param>
    /// <returns></returns>
    Task<string> ChangePassword(string idToken, ChangePasswordDto changePassword);
}