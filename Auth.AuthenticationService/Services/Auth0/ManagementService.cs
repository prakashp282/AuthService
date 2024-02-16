using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Auth.Models.Common;
using Auth.Models.Dtos;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Auth.AuthenticationService.Services.Auth0
{
    public class ManagementService : IManagementService
    {
        #region Private

        private readonly ILogger _logger;
        private static DateTime TokenExpirationTime { get; set; } = DateTime.MinValue;
        private static string AccessToken { get; set; }
        private static ManagementApiClient managementApiClient { get; set; }
        private readonly string _tokenEndpoint = $"https://{Globals.Auth0Domain}/oauth/token";
        private readonly string _audience = $"https://{Globals.Auth0Connection}/api/v2/";


        /// <summary>
        /// Rotate Managment access token using the correct audience.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> RefreshAccessToken()
        {
            try
            {
                using HttpClient client = new();
                string clientId = Globals.Auth0ClientId;
                string clientSecret = Globals.Auth0ClientSecret;

                var requestBody = new
                {
                    grant_type = GrantType.ClientCredentials,
                    client_id = clientId,
                    client_secret = clientSecret,
                    audience = _audience
                };

                StringContent requestContent = new StringContent(JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(_tokenEndpoint, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    AccessTokenDto tokenDtoResponse = JsonConvert.DeserializeObject<AccessTokenDto>(responseContent);

                    // Set the new access token expiration time
                    TokenExpirationTime = DateTime.UtcNow.AddSeconds(tokenDtoResponse.ExpiresIn);

                    return tokenDtoResponse.AccessToken;
                }
                else
                {
                    throw new Exception($"Failed to refresh access token. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        #endregion

        #region Public Methods

        public ManagementService(ILogger<ManagementService> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Get an active Managment token.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetManagementToken()
        {
            if (DateTime.UtcNow >= TokenExpirationTime)
            {
                // Access token is expired or not available, obtain a new one.
                AccessToken = await RefreshAccessToken();
            }

            return AccessToken;
        }

        /// <summary>
        /// Gets an Active Managment api client.
        /// </summary>
        /// <returns></returns>
        public async Task<ManagementApiClient> GetManagmentApiClient()
        {
            if (managementApiClient == null || DateTime.UtcNow >= TokenExpirationTime)
            {
                //Reinitialize on expiry.
                managementApiClient = new ManagementApiClient(await GetManagementToken(), new Uri(_audience));
            }

            return managementApiClient;
        }

        /// <summary>
        /// Checks if the provided Phone number is Unique or not.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public async Task<bool> IsPhoneNumberUnique(string phoneNumber)
        {
            try
            {
                string filter = $"user_metadata.phone_number:\"{phoneNumber}\"";
                string userMetadataFilterField = "user_metadata";
                GetUsersRequest userRequest = new GetUsersRequest()
                {
                    Fields = userMetadataFilterField,
                    Query = filter
                };
                managementApiClient = await GetManagmentApiClient();
                var users = await managementApiClient?.Users?.GetAllAsync(userRequest)!;
                return users?.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in verifing phone number " + ex);
                throw;
            }
        }

        #endregion
    }
}