using Newtonsoft.Json;

namespace Auth.Models.Dtos
{
    public class AccessTokenDto
    {
        /// <summary>Access token.</summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>Type of token.</summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>Identifier token.</summary>
        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        /// <summary>Expiration time in seconds.</summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>Refresh token.</summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// If we need to show the Otp screen
        /// </summary>
        public bool NeedsMFA { get; set; } = false;

        /// <summary>
        /// MFA token.
        /// </summary>
        [JsonProperty("mfa_token")]
        public string MFAToken { get; set; }

        /// <summary>
        /// Challenge Id recieved for MFA verfication; for sms - oob code
        /// </summary>
        public string ChallengeId { get; set; }
    }
}