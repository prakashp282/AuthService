using System;
using System.Linq;
using System.Threading.Tasks;
using Auth.Models.Dtos;
using Newtonsoft.Json;
using RestSharp;

namespace Auth.AuthenticationService.Services.Auth0
{
    public class MfaService : IMFAService
    {
        private static readonly RestClient Client = new RestClient();
        private static readonly string _mfaAssociateRequestUrl = $"https://{Globals.Auth0Domain}/mfa/associate";
        private static readonly string _oob = "oob";
        private static readonly string[] _authenticatorTypes = [_oob];
        private static readonly string[] _oobChannelStrings = ["sms"];
        private static readonly string _verifyChallengeRequestUrl = $"https://{Globals.Auth0Domain}/oauth/token";
        private static readonly string _mfaGrantType = "http://auth0.com/oauth/grant-type/mfa-oob";
        private static readonly string _initiateMfaChallengeUrl = $"https://{Globals.Auth0Domain}/mfa/challenge";


        /// <summary>
        /// Enroll/Associate a Phone number to user to send verification code using Auth0 api 
        /// </summary>
        /// <param name="mFAToken"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public async Task<string> EnrollPhoneNumberAsync(string mfaToken, string phoneNumber)
        {
            //TODO: add check already enrolled logic
            try
            {
                RestRequest request = new RestRequest(_mfaAssociateRequestUrl, Method.Post);
                request.AddHeader("authorization", $"Bearer {mfaToken}");
                request.AddHeader("content-type", "application/json");
                var body = new
                {
                    client_id = Globals.Auth0ClientId,
                    client_secret = Globals.Auth0ClientSecret,
                    authenticator_types = _authenticatorTypes,
                    oob_channels = _oobChannelStrings,
                    phone_number = phoneNumber
                };
                //"{ \"authenticator_types\": [\"oob\"], \"oob_channels\": [\"sms\"], \"phone_number\": \"+91{phoneNumber}\" }"
                request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
                RestResponse response = await Client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    return responseObject.oob_code;
                }
                else
                {
                    // Handle error based on response status code
                    throw new Exception($"MFA association failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions gracefully
                throw new Exception("Error associating MFA: " + ex.Message);
            }
        }

        /// <summary>
        /// Initiate a request to send verification message to associated phone number.
        /// </summary>
        /// <param name="mFAToken"></param>
        /// <returns></returns>
        public async Task<AccessTokenDto> VerifyMfaChallengeAsync(string mfaToken, string ChallengeId,
            string verificationCode)
        {
            try
            {
                var request = new RestRequest(_verifyChallengeRequestUrl, Method.Post);
                request.AddHeader("authorization", $"Bearer {mfaToken}");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                // "grant_type=http%3A%2F%2Fauth0.com%2Foauth%2Fgrant-type%2Fmfa-oob&client_id={yourClientId}&client_secret=%7ByourClientSecret%7D&mfa_token=%7BmfaToken%7D&oob_code=%7BoobCode%7D&binding_code=%7BuserOtpCode%7D"

                var requestData = new System.Collections.Specialized.NameValueCollection();
                requestData.Add("client_id", Globals.Auth0ClientId);
                requestData.Add("client_secret", Globals.Auth0ClientSecret);
                requestData.Add("mfa_token", mfaToken);
                requestData.Add("grant_type", _mfaGrantType);
                requestData.Add("oob_code", ChallengeId);
                requestData.Add("binding_code", verificationCode);

                var requestBody = string.Join("&", requestData.AllKeys.Select(key => $"{key}={requestData[key]}"));
                request.AddParameter("application/x-www-form-urlencoded", requestBody
                    , ParameterType.RequestBody);

                var response = await Client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<AccessTokenDto>(response.Content);
                }
                else
                {
                    // Handle error based on response status code
                    //throw new ArgumentException($"MFA association failed with status code: {response.Content}");
                    throw new ArgumentException("invalid_mfa_token");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to verify Challenge: {ex.Message}");
            }
        }


        /// <summary>
        /// Verifies the the user entere verification code/otp.
        /// </summary>
        /// <param name="mfaToken"></param>
        /// <param name="challengeId"></param>
        /// <param name="otp"></param>
        /// <returns></returns>
        public async Task<string> InitiateMfaChallengeAsync(string mfaToken)
        {
            RestRequest request = new RestRequest(_initiateMfaChallengeUrl, Method.Post);
            request.AddHeader("content-type", "application/json");
            // "{ \"client_id\": \"{yourClientId}\",  \"client_secret\": \"{yourClientSecret}\", \"challenge_type\": \"oob\", \"authenticator_id\": \"sms|dev_NU1Ofuw3Cw0XCt5x\", \"mfa_token\": \"{mfaToken}\" }"
            var body = new
            {
                client_id = Globals.Auth0ClientId,
                client_secret = Globals.Auth0ClientSecret,
                mfa_token = mfaToken,
                challenge_type = _oob,
            };
            request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
            RestResponse response = await Client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"MFA initiation failed with status code: {response.StatusCode}");
            }

            var responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return responseObject.oob_code;
        }
    }
}