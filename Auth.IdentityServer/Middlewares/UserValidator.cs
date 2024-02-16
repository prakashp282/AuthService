using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using IdentityServer.Exceptions;
using IdentityServer.Models;
using IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using static IdentityModel.OidcConstants;

namespace IdentityServer;

/// <summary>
/// Extending Resource Owner Password Credential Grant flow to enable MFA 
/// </summary>
public class UserValidator : IResourceOwnerPasswordValidator
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private IAccountService _accountService;
    private ILogger<UserValidator> _logger;

    public UserValidator(
        IAccountService accountService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<UserValidator> logger)
    {
        _accountService = accountService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        // context set to Failure        
        context.Result = new GrantValidationResult(
            TokenRequestErrors.UnauthorizedClient, "Invalid Credentials");
        try
        {
            var result = await _signInManager.PasswordSignInAsync(context.UserName, context.Password, true, true);

            if (result.Succeeded)
            {
                //Without MFA.
                var user = await _userManager.FindByNameAsync(context.UserName);
                if (user != null)
                {
                    var claims = await _userManager.GetClaimsAsync(user);
                    // context set to success
                    context.Result = new GrantValidationResult(
                        user.Id,
                        AuthenticationMethods.Password,
                        claims
                    );
                    return;
                }
            }
            else if (result.RequiresTwoFactor)
            {
                ApplicationUser? user = await _userManager.FindByNameAsync(context.UserName);
                // Generate new MFA code and Send it
                await _accountService.SendCode(user, ["Phone"]);
                // custom error message
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidRequest, "mfa_required");
            }
            else if (result.IsLockedOut)
            {
                //User is locked out
                //_logger.LogWarning("User Account locked out");
                throw new UserLockedOutException("user Account Locked Out");
            }
            else
            {
                throw new UserSignInException("User Invalid Credentials");
            }
        }
        catch (Exception e)
        {
            // ignored
            _logger.LogError(e, "Error while validating ROPC");
        }
    }
}