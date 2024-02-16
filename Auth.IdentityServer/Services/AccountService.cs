using Auth.Models.Dtos;
using IdentityServer.Exceptions;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json;

namespace IdentityServer.Services;

public class AccountService : IAccountService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ISmsSender _smsSender;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ISmsSender smsSender,
        ILogger<AccountService> logger,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _logger = logger;
    }

    public async Task<UserInfoDto> Register(SignUpDto signUp)
    {
        //Check if user already exists.
        await CheckUserExists(signUp.Email);

        //USING EMAIL AS USER NAME, later as we want ot logs in by MemberId not USER NAME need to migrate
        var user = new ApplicationUser
        {
            UserName = signUp.Email,
            Email = signUp.Email,
            PhoneNumber = signUp.PhoneNumber,
            TwoFactorEnabled = true, //For MFA purpose
            PhoneNumberConfirmed = true //For testing purpose
        };

        //Create user
        var result = await _userManager.CreateAsync(user, signUp.Password);
        if (!result.Succeeded)
        {
            throw new UserCreationFailedException("Failed to Create User" + result.ToString());
        }

        //Add Token to Verify the username....
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //Generate appropriate url.
        //var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, username = user.Email }, Request.Scheme);
        var confirmationLink =
            $"{Environment.GetEnvironmentVariable("SERVER_URL")}/api/ConfirmEmail?token={token}&username={user.Email}";
        await _emailSender.SendEmailAsync(user.Email!, "Confirmation username link",
            "Please confirm your account by clicking here: <a href=\"" + confirmationLink + "\">link</a>");
        //Adding Default role
        await this.AddRole(new UserRoleDto(user.Email, "Default"));

        //Explicit Casting
        return JsonConvert.DeserializeObject<UserInfoDto>(JsonConvert.SerializeObject(user));
    }

    public async Task<string> ConfirmMail(string token, string email)
    {
        //get user
        var user = await FindUserByUserName(email);
        //Set username field as confiremed.
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            throw new UserVerificationException("Failed to verify user" + result.ToString());
        }

        return "Email Verified Successfully";
    }

    public async Task<string> UpdateUser(UserInfoDto user)
    {
        //Get Current user.
        var userToUpdate = await FindUserById(user.UserId);
        // Map request data to user properties
        userToUpdate.UserName = user.NickName;
        // ... other properties
        var result = await _userManager.UpdateAsync(userToUpdate);
        if (!result.Succeeded)
        {
            throw new UserUpdateFailedException("Failed to Update User" + result.ToString());
        }

        return "User Update Successfully";
    }

    public async Task<string> ChangePassword(ChangePasswordDto changePassword)
    {
        var user = await FindUserByUserName(changePassword.Email);
        var result = await _userManager.ChangePasswordAsync(user, changePassword.Password, changePassword.NewPassword);
        if (!result.Succeeded)
        {
            throw new PasswordChangeException("Error while updating password" + result.ToString());
        }

        return "Password Changed!";
    }

    public async Task<string> ResetPassword(ForgotPasswordDto resetPassword)
    {
        var user = await FindUserByUserName(resetPassword.Email);

        // Send an email with this link
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        //Generate appropriate reset Link.
        var resetLink =
            $"{Environment.GetEnvironmentVariable("SERVER_URL")}/api/ResetPassword?userId={user.Id}&code={code}";

        await _emailSender.SendEmailAsync(user.Email, "Reset Password",
            "Please reset your password by clicking here: <a href=\"" + resetLink + "\">link</a>");

        _logger.LogInformation("Password reset Mail Sent");

        return "Password Reset Mail Sent!";
    }

    public async Task SendCode(ApplicationUser user, List<string> tokenProvider = null)
    {
        var providers = tokenProvider ?? await _userManager.GetValidTwoFactorProvidersAsync(user);
        if (providers.Any(s => s == "Email"))
        {
            _logger.LogInformation("Sending Verification Mail");
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            var message = "<h1> Your security code is: " + code + "</h1>";
            await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
        }
        else if (providers.Any(s => s == "Phone"))
        {
            _logger.LogInformation("Sending Verification SMS");
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
            var message = "Your security code is: " + code;
            await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
        }
    }

    public async Task<string> VerifyCode(VerifyChallengeDto verifyChallenge)
    {
        //get user fom the mfa token/Identity.TwoFactorUserIdScheme
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
        var provider = providers.FirstOrDefault();

        // The following code protects for brute force attacks against the two factor codes.
        // If a user enters incorrect codes for a specified amount of time then the user account
        // will be locked out for a specified amount of time.
        var result = await _signInManager.TwoFactorSignInAsync(provider, verifyChallenge.Otp, false, false);

        if (result.IsLockedOut)
        {
            _logger.LogError($"User {user.Email} account locked out.");
            throw new UserLockedOutException("User Locked out! " + result.ToString());
        }
        else if (!result.Succeeded)
        {
            _logger.LogError("Account Verification Error");
            throw new UserVerificationException("Error while Validating user " + result.ToString());
        }

        _logger.LogInformation("Code Verified");
        return "Code verified";
    }

    public async Task<string> AddRole(UserRoleDto userRole)
    {
        //Check user
        var user = await this.FindUserByUserName(userRole.Email);
        //checking if role exists
        var role = await this.CheckRoleExist(userRole.Role);
        //Add Role
        IdentityResult result = await _userManager.AddToRoleAsync(user, role.Name);

        if (!result.Succeeded)
        {
            _logger.LogError("Error while adding role" + result.ToString());
            throw new AddRoleException("There was an error adding role " + result.ToString());
        }

        return "Role added to user";
    }

    public async Task<string> RemoveRole(UserRoleDto userRole)
    {
        //Check user
        var user = await this.FindUserByUserName(userRole.Email);
        //checking if role exists
        var role = await this.CheckRoleExist(userRole.Role);
        //Remove Role
        IdentityResult result = await _userManager.RemoveFromRoleAsync(user, role.Name);

        if (!result.Succeeded)
        {
            _logger.LogError("Error while removing role" + result.ToString());
            throw new RemoveRoleException("There was an error removing role " + result.ToString());
        }

        return "Role Removed from user";
    }

    private async Task CheckUserExists(string username)
    {
        var user = await _userManager.FindByEmailAsync(username);
        if (user != null)
        {
            throw new UserAlreadyExists("Cannot create user, already Exist");
        }
    }

    private async Task<ApplicationUser> FindUserByUserName(string username)
    {
        //Check if User Exist 
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            throw new UserDoesNotExistException("User does not exists");
        }

        return user;
    }

    private async Task<ApplicationUser> FindUserById(string userId)
    {
        //Check if User Exist 
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UserDoesNotExistException("User does not exists");
        }

        return user;
    }

    private async Task<IdentityRole> CheckRoleExist(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            throw new RoleDoesNotExistException("Role does not exists");
        }

        return role;
    }
}