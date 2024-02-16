using Auth.Models.Dtos;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    [Route("/")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("Register")]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(SignUpDto signUp)
        {
            var user = _accountService.Register(signUp);
            return Ok(user);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            return Ok(new IdentityResponseDto() { Message = await _accountService.ConfirmMail(token, email) });
        }

        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UserInfoDto user)
        {
            return Ok(new IdentityResponseDto() { Message = await _accountService.UpdateUser(user) });
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePassword)
        {
            return Ok(new IdentityResponseDto() { Message = await _accountService.ChangePassword(changePassword) });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ForgotPasswordDto forgotPassword)
        {
            return Ok(new IdentityResponseDto() { Message = await _accountService.ResetPassword(forgotPassword) });
        }

        [HttpPost("SendCode")]
        public async Task<IActionResult> SendCode(dynamic model)
        {
            await _accountService.SendCode(model ?? new List<string>
            {
                "Phone"
            });
            return Ok();
        }

        [HttpPost("VerifyCode")]
        public async Task<IActionResult> VerifyCode(VerifyChallengeDto? verifyChallengeDto)
        {
            return Ok(new IdentityResponseDto() { Message = await _accountService.VerifyCode(verifyChallengeDto) });
        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRole(UserRoleDto userRole)
        {
            return Ok(new IdentityResponseDto() { Message = await _accountService.AddRole(userRole) });
        }

        [HttpPost("RemoveRole")]
        public async Task<IActionResult> RemoveRole(UserRoleDto userRole)
        {
            return Ok(new IdentityResponseDto() { Message = await _accountService.RemoveRole(userRole) });
        }
    }
}