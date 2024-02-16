using System.Threading.Tasks;
using Auth.Models.Dtos;

namespace Auth.AuthenticationService.Services.Auth0
{
    public interface IMFAService
    {
        /// <summary>
        /// Enroll/Associate a Phone number to user to send verification code.
        /// </summary>
        /// <param name="mFAToken"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        Task<string> EnrollPhoneNumberAsync(string mFAToken, string phoneNumber);

        /// <summary>
        /// Initiate a request to send verification message to associated phone number.
        /// </summary>
        /// <param name="mFAToken"></param>
        /// <returns></returns>
        Task<string> InitiateMfaChallengeAsync(string mFAToken);

        /// <summary>
        /// Verifies the the user entere verification code/otp.
        /// </summary>
        /// <param name="mfaToken"></param>
        /// <param name="challengeId"></param>
        /// <param name="otp"></param>
        /// <returns></returns>
        Task<AccessTokenDto> VerifyMfaChallengeAsync(string mfaToken, string challengeId, string otp);
    }
}