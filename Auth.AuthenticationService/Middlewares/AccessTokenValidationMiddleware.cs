using System;
using System.Linq;
using System.Threading.Tasks;
using Auth.AuthenticationService.Services;
using Auth.AuthenticationService.Services.Common;
using Auth0.Core.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Auth.AuthenticationService.Middlewares
{
    public class AccessTokenValidationMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private IAuthenticationProvider _authenticationProvider;

        public AccessTokenValidationMiddleware(RequestDelegate next, ILogger<AccessTokenValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IAuthenticationProvider authenticationProvider)
        {
            _authenticationProvider = authenticationProvider;
            try
            {
                var claims = context?.User?.Claims;
                var refreshToken = context.Request.Cookies["refreshToken"];
                if (!claims.Any() || String.IsNullOrEmpty(refreshToken))
                {
                    await _next(context);
                    return;
                }

                // Validate the token using JwtBearer middleware
                await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                var expiryClaim = claims.FirstOrDefault(c => c.Type == "exp");
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiryClaim.Value)).UtcDateTime;
                if (expiryClaim != null && DateTime.UtcNow > expirationTime)
                {
                    var accessToken = await _authenticationProvider.GetValidAccessToken(refreshToken);
                    if (accessToken.RefreshToken == null)
                    {
                        accessToken.RefreshToken = refreshToken;
                    }

                    CookieHelper.BindCookies(context, accessToken);
                }
            }
            catch (ApiException ex)
            {
                // Handle validation/refresh errors
                _logger.LogError(ex, "Error while refreshing AccessToken");
                context.Response.StatusCode = 401;
                throw;
            }

            await _next(context);
        }
    }

    public static class AccessTokenValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAccessTokenValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AccessTokenValidationMiddleware>();
        }
    }
}