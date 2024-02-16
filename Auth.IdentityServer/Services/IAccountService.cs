using Auth.Models.Dtos;
using IdentityServer.Models;

namespace IdentityServer.Services;

public interface IAccountService
{
    Task<UserInfoDto> Register(SignUpDto signUp);
    Task<string> ConfirmMail(string token, string email);
    Task<string> UpdateUser(UserInfoDto user);
    Task<string> ChangePassword(ChangePasswordDto changePassword);
    Task<string> ResetPassword(ForgotPasswordDto resetPassword);
    Task SendCode(ApplicationUser user, List<string> tokenProvider = null);
    Task<string> VerifyCode(VerifyChallengeDto verifyChallenge);
    Task<string> AddRole(UserRoleDto userRole);
    Task<string> RemoveRole(UserRoleDto userRole);
}