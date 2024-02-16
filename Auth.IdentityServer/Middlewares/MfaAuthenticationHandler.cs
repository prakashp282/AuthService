using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using static IdentityModel.OidcConstants;

namespace IdentityServer;

/// <summary>
/// Extending Grant type to handle Mfa Verification.
/// </summary>
public class AuthenticationGrant : IExtensionGrantValidator
{
    private readonly ILogger<AuthenticationGrant> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationGrant(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        ILogger<AuthenticationGrant> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public string GrantType => "MfaVerification";


    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        // context set to Failure        
        context.Result = new GrantValidationResult(
            TokenRequestErrors.UnauthorizedClient, "MFA token mismatch");

        var code = context.Request.Raw.Get("code");
        try
        {
            //Using the cookie we get the UserId and set it in context.
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user != null)
            {
                //Now we verify the code. This will self handle brute force attack
                var result = await _signInManager.TwoFactorSignInAsync("Phone", code, false, false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                    var claims = await _userManager.GetClaimsAsync(user);
                    // context set to success
                    context.Result = new GrantValidationResult(
                        user.Id,
                        AuthenticationMethods.MultiFactorAuthentication,
                        claims
                    );
                    return;
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while validating Mfa");
        }
    }
}