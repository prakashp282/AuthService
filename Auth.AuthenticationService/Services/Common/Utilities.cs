using System;
using System.IdentityModel.Tokens.Jwt;
using Auth.Models.Dtos;
using Newtonsoft.Json;

namespace Auth.AuthenticationService.Services.Common
{
    public static class Utilities
    {
        public static UserInfoDto GetUserFormIdToken(string idToken)
        {
            // Create a JwtSecurityTokenHandler instance
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            // Read the token into a JwtSecurityToken object
            JwtSecurityToken jwtTokenObject = tokenHandler.ReadJwtToken(idToken);
            // Access the payload as a JSON string
            string payloadJson = jwtTokenObject.Payload.SerializeToJson().ToString();
            // Optionally, deserialize the payload into a dynamic object
            // for easier access to individual claims:
            return JsonConvert.DeserializeObject<UserInfoDto>(payloadJson);
        }

        public static string GetUserIdFromToken(string idToken)
        {
            try
            {
                return GetUserFormIdToken(idToken).UserId;
            }
            catch (Exception ex)
            {
                // Handle exceptions during decoding or parsing
                throw new Exception("Error extracting ID from token", ex);
            }
        }
    }
}