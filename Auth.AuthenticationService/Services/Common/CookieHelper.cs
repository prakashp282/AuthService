using System;
using Auth.Models.Dtos;
using Microsoft.AspNetCore.Http;

namespace Auth.AuthenticationService.Services.Common;

public static class CookieHelper
{
    /// <summary>
    /// Unset All cookies.
    /// </summary>
    public static void UnbindCookies(HttpContext context)
    {
        //For authentication
        context.Response.Cookies.Delete("idToken");

        //For authorization 
        context.Response.Cookies.Delete("token");
        context.Response.Cookies.Delete("refreshToken");

        //For mfa verification
        context.Response.Cookies.Delete("mfaToken");
        context.Response.Cookies.Delete("challengeId");
    }

    /// <summary>
    /// Set the cookies found in token.
    /// </summary>
    /// <param name="tokenDto"></param>
    public static void BindCookies(HttpContext context, AccessTokenDto tokenDto)
    {
        //Clear previous cookies
        //UnbindCookies();
        if (!String.IsNullOrEmpty(tokenDto.AccessToken))
        {
            context.Response.Cookies.Append("token", tokenDto.AccessToken,
                new CookieOptions
                {
                    //Expires = DateTime.Now.AddSeconds(token?.ExpiresIn ?? 4 * 60 * 60),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }

        if (!String.IsNullOrEmpty(tokenDto.IdToken))
        {
            context.Response.Cookies.Append("idToken", tokenDto.IdToken,
                new CookieOptions
                {
                    //Expires = DateTime.Now.AddSeconds(token?.ExpiresIn ?? 4 * 60 * 60),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }

        if (!String.IsNullOrEmpty(tokenDto.MFAToken))
        {
            context.Response.Cookies.Append("mfaToken", tokenDto.MFAToken,
                new CookieOptions
                {
                    //Expires = DateTime.Now.AddSeconds(token?.ExpiresIn ?? 4 * 60 * 60),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }

        if (!String.IsNullOrEmpty(tokenDto.ChallengeId))
        {
            context.Response.Cookies.Append("challengeId", tokenDto.ChallengeId,
                new CookieOptions
                {
                    //Expires = DateTime.Now.AddSeconds(token?.ExpiresIn ?? 4 * 60 * 60),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }

        if (!String.IsNullOrEmpty(tokenDto.RefreshToken))
        {
            context.Response.Cookies.Append("refreshToken", tokenDto.RefreshToken,
                new CookieOptions
                {
                    //Expires = DateTime.Now.AddSeconds(token?.ExpiresIn ?? 4 * 60 * 60),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }
    }
}