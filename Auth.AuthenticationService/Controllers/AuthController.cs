using System;
using System.Linq;
using System.Threading.Tasks;
using Auth.AuthenticationService.Services;
using Auth.AuthenticationService.Services.Common;
using Auth.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.AuthenticationService.Controllers
{
    /// <summary>
    /// This is for user Authentication.
    /// </summary>
    [Route("/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationProvider _authenticationProvider;

        public AuthController(IAuthenticationProvider authenticationProvider)
        {
            _authenticationProvider = authenticationProvider;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignUpDto signUp)
        {
            AccessTokenDto tokenDto = await _authenticationProvider.SignUpWithMFAAsync(signUp);
            //TODO : SEARCH AND MOVE TO RESPONSE INSTEAD OF COOKIES
            CookieHelper.BindCookies(HttpContext, tokenDto);
            ApiResponseDto apiResponse =
                new ApiResponseDto(message: "User signup Successful", data: new { tokenDto.NeedsMFA });
            return Ok(apiResponse);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromBody] SignInDto signIn)
        {
            AccessTokenDto tokenDto = await _authenticationProvider.SignInWithMFAAsync(signIn);
            CookieHelper.BindCookies(HttpContext, tokenDto);
            ApiResponseDto apiResponse =
                new ApiResponseDto(message: "User signin Successful", data: new { tokenDto.NeedsMFA });
            return Ok(apiResponse);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            var response =
                new ApiResponseDto(message: await _authenticationProvider.ResetPasswordAsync(forgotPasswordDto.Email));
            return Ok(response);
        }

        [HttpGet("sign-out")]
        [Authorize]
        public async Task<IActionResult> Signout()
        {
            //Clearing local session 
            CookieHelper.UnbindCookies(HttpContext);
            // generate Redirect user to the Auth0 logout page
            string redirectUri = await _authenticationProvider.SignOutAsync();
            ApiResponseDto apiResponse = new ApiResponseDto(message: "User Logged out", data: new { redirectUri });
            return Ok(apiResponse);
        }

        [HttpPost("verify-mfa")]
        public async Task<IActionResult> VerifyMFA([FromBody] VerifyChallengeDto verifyChallenge)
        {
            string mfaToken = HttpContext.Request.Cookies["mfaToken"];
            string challengeId = HttpContext.Request.Cookies["challengeId"];

            AccessTokenDto tokenDto =
                await _authenticationProvider.VerifyMfaChallengeAsync(mfaToken, challengeId, verifyChallenge);
            CookieHelper.BindCookies(HttpContext, tokenDto);
            ApiResponseDto apiResponse = new ApiResponseDto
                (message: "User signin Successful");
            return Ok(apiResponse);
        }

        //Helper Method to vaditate and reissue token if needed.
        [HttpGet("validate-token")]
        [Authorize]
        public async Task<IActionResult> ValidateToken(bool forceRefresh = false)
        {
            var claims = HttpContext?.User?.Claims;
            if (!claims.Any())
            {
                throw new UnauthorizedAccessException("Tried to Validate token without token");
            }

            var expiryClaim = claims.FirstOrDefault(c => c.Type == "exp");
            if (forceRefresh || expiryClaim != null && DateTime.UtcNow > Convert.ToDateTime(expiryClaim.Value))
            {
                var refreshToken = HttpContext.Request.Cookies["refreshToken"];
                var token = await _authenticationProvider.GetValidAccessToken(refreshToken);
                if (token != null)
                {
                    CookieHelper.BindCookies(HttpContext, token);
                }
            }

            var response = new ApiResponseDto(message: "Token Validated");
            return Ok(response);
        }
    }
}