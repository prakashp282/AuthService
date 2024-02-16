using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auth.Models.Dtos;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Auth.AuthenticationService.Services.Auth0
{
    public class Auth0Service : IAuthenticationProvider
    {
        private readonly IAuthenticationApiClient _authenticationApiClient;

        private readonly ILogger _logger;
        private readonly IMFAService _mFaService;

        private readonly string _phoneNumberString = "phone_number";
        private IManagementService _managementService;

        public Auth0Service(IMFAService mFaService, IManagementService managementService, ILogger<Auth0Service> logger)
        {
            // Initialize Auth0 client with your Auth0 credentials
            _authenticationApiClient = new AuthenticationApiClient(Globals.Auth0Domain);
            _managementService = managementService;
            _mFaService = mFaService;
            _logger = logger;
        }

        /// <summary>
        /// Request new user signup and then tries to log in the user too. If MFA is required it enrolls the user phone number
        /// </summary>
        /// <param name="signUp"></param>
        /// <returns></returns>
        public async Task<AccessTokenDto> SignUpWithMFAAsync(SignUpDto signUp)
        {
            //Check phone's uniqueness
            if (!(await _managementService.IsPhoneNumberUnique(signUp.PhoneNumber)))
            {
                _logger.LogWarning("User tried to sign up with duplicate phone number");
                throw new ArgumentException("phone_mapped");
            }

            // Signup user using Auth0 API
            var signUpRequest = new SignupUserRequest
            {
                Connection = Globals.Auth0Connection,
                Email = signUp.Email,
                Password = signUp.Password,
                Username = signUp.UserName,
                UserMetadata =
                    new Dictionary<string, object>()
                    {
                        { _phoneNumberString, signUp.PhoneNumber }
                    }
            };

            try
            {
                //SignUp user.
                var result = await _authenticationApiClient.SignupUserAsync(signUpRequest);

                //Now SigIn to get MFA token
                var response = await SignInAsync(new SignInDto
                {
                    Email = signUp.Email,
                    Password = signUp.Password,
                });

                //If we see any MFA token then we enroll and verify the Code.
                if (String.IsNullOrEmpty(response.AccessToken) && !String.IsNullOrEmpty(response.MFAToken))
                {
                    response.NeedsMFA = true;
                    response.ChallengeId =
                        await _mFaService.EnrollPhoneNumberAsync(response.MFAToken, signUp.PhoneNumber);
                }

                _logger.LogInformation($"User {signUp.Email} SignUp successfull");
                return response;
            }
            catch (ErrorApiException apiException)
            {
                if (apiException.ApiError.Error == "")
                {
                    AccessTokenDto response = new();
                    response.MFAToken = apiException.ApiError.ExtraData["mfa_token"];
                    return response;
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Sign up Error : " + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Request user signin to auth0. If MFA is required it initiates a challenge to send sms to the enrolled phone number
        /// </summary>
        /// <param name="signIn"></param>
        /// <returns></returns>
        public async Task<AccessTokenDto> SignInWithMFAAsync(SignInDto signIn)
        {
            try
            {
                AccessTokenDto response = await SignInAsync(signIn);
                //If we see any MFA token then we enroll and verify the Code.
                if (String.IsNullOrEmpty(response.AccessToken) && !String.IsNullOrEmpty(response.MFAToken))
                {
                    response.NeedsMFA = true;
                    response.ChallengeId = await _mFaService.InitiateMfaChallengeAsync(response.MFAToken);
                    _logger.LogInformation($"MFA Request for  {signIn.Email} Sent successfully");
                }
                else
                {
                    _logger.LogInformation($"User {signIn.Email} SignIn successfully");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Sign In Error : " + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// The will use the verifyChallenge otp and mfa token calling the auth0 to verify if the otp is correct.
        /// </summary>
        /// <param name="mfaToken"></param>
        /// <param name="challengeId"></param>
        /// <param name="verifyChallenge"></param>
        /// <returns></returns>
        public async Task<AccessTokenDto> VerifyMfaChallengeAsync(string mfaToken, string challengeId,
            VerifyChallengeDto verifyChallenge)
        {
            try
            {
                var result =
                    await _mFaService.VerifyMfaChallengeAsync(mfaToken, challengeId,
                        verifyChallenge.Otp); // Process result as needed
                _logger.LogInformation($"User Successfuly Verified");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Generates a Logout URL then user is redirected to the url in frontend, effectively logging them out of Auth0.
        /// </summary>
        /// <returns></returns>
        public async Task<string> SignOutAsync()
        {
            try
            {
                // Build the logout URI - will logout user from Auth0 - and invalidate tokens
                var logoutUri = _authenticationApiClient.BuildLogoutUrl().Build().ToString();
                return logoutUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// This will trigger a reset password mail from auth0 and user would then be able to reset password using the email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<string> ResetPasswordAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Password reset requested for  {email}");

                var changePasswordRequest = new ChangePasswordRequest
                {
                    Email = email,
                    Connection = Globals.Auth0Connection
                };
                // Send a password change request to Auth0
                return await _authenticationApiClient.ChangePasswordAsync(changePasswordRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }


        /// <summary>
        /// Validate Access Token and assign new with refresh token if invalid
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public async Task<AccessTokenDto> GetValidAccessToken(string refreshToken)
        {
            try
            {
                // If invalid, attempt to refresh the token
                var refreshTokenRequest = new RefreshTokenRequest()
                {
                    ClientId = Globals.Auth0ClientId,
                    ClientSecret = Globals.Auth0ClientSecret,
                    RefreshToken = refreshToken,
                };
                var refreshResponse = await _authenticationApiClient.GetTokenAsync(refreshTokenRequest);
                string jsonString = JsonConvert.SerializeObject(refreshResponse);
                AccessTokenDto accessTokenDto = JsonConvert.DeserializeObject<AccessTokenDto>(jsonString);
                return accessTokenDto;
            }
            catch (Exception ex)
            {
                // Handle errors, such as invalid refresh token or network issues
                // Log the error and potentially trigger user re-authentication
                _logger.LogError(message: "Error while getting Refresh token " + ex.Message);
                throw;
            }
        }


        /// <summary>
        /// Request Sign in to auth0, with Resource Owner Token as we have absolute control, and either return Token or an MFA required exception.
        /// </summary>
        /// <param name="signIn"></param>
        /// <returns></returns>
        public async Task<AccessTokenDto> SignInAsync(SignInDto signIn)
        {
            // Signin user using Auth0 API
            var tokenRequest = new ResourceOwnerTokenRequest
            {
                ClientId = Globals.Auth0ClientId,
                ClientSecret = Globals.Auth0ClientSecret,
                Realm = Globals.Auth0Connection,
                Audience = Globals.Auth0Audience,
                Scope = Globals.RequestScope,
                Username = signIn.Email,
                Password = signIn.Password,
            };

            try
            {
                var serializedParent =
                    JsonConvert.SerializeObject(await _authenticationApiClient.GetTokenAsync(tokenRequest));
                AccessTokenDto result = JsonConvert.DeserializeObject<AccessTokenDto>(serializedParent);

                _logger.LogInformation($"User {signIn.Email} Logged In successfull");

                return result;
            }
            catch (ErrorApiException apiException)
            {
                if (apiException.ApiError.Error == "mfa_required")
                {
                    AccessTokenDto response = new();
                    response.MFAToken = apiException.ApiError.ExtraData["mfa_token"];
                    _logger.LogInformation($"Needs MFA Verification");
                    return response;
                }

                _logger.LogError(apiException, apiException.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while signing in");
                throw;
            }
        }
    }
}