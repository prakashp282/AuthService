using System.Threading.Tasks;
using Auth.Models.Dtos;

namespace Auth.AuthenticationService.Services
{
    public partial interface IAuthenticationProvider
    {
        /// <summary>
        /// Request new user signup and then tries to log in the user too. If MFA is required it enrolls the user phone number
        /// </summary>
        /// <param name="signUp"></param>
        /// <returns></returns>
        Task<AccessTokenDto> SignUpWithMFAAsync(SignUpDto signUp);

        /// <summary>
        /// Request user signin to auth0. If MFA is required it initiates a challenge to send sms to the enrolled phone number
        /// </summary>
        /// <param name="signIn"></param>
        /// <returns></returns>
        Task<AccessTokenDto> SignInWithMFAAsync(SignInDto signIn);

        /// <summary>
        /// The will use the verifyChallenge otp and mfa token calling the auth0 to verify if the otp is correct.
        /// </summary>
        /// <param name="mfaToken"></param>
        /// <param name="challengeId"></param>
        /// <param name="verifyChallenge"></param>
        /// <returns></returns>
        Task<AccessTokenDto> VerifyMfaChallengeAsync(string mfaToken, string challengeId,
            VerifyChallengeDto verifyChallenge);

        /// <summary>
        /// This will trigger a reset password mail from IdentityProvider and user would then be able to reset password using the email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<string> ResetPasswordAsync(string email);

        /// <summary>
        /// Generates a Logout URL then user is redirected to the url in frontend, effectively logging them out of Auth0.
        /// </summary>
        /// <returns></returns>
        Task<string> SignOutAsync();

        /// <summary>
        /// Validate Access Token and assign new with refresh token if invalid
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        Task<AccessTokenDto> GetValidAccessToken(string refreshToken);
    }
}