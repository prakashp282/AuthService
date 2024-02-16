using System.Threading.Tasks;
using Auth.AuthenticationService.Services;
using Auth.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.AuthenticationService.Controllers;

[Route("/user")]
[ApiController]
public class UserController : ControllerBase
{
    private IUserManager _userManager;

    public UserController(IUserManager userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUser()
    {
        HttpContext.Request.Cookies.TryGetValue("token", out var accessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequest(new ApiResponseDto(status: 404, error: "Invalid Access Token"));
        }

        var user = await _userManager.User(accessToken);
        ApiResponseDto apiResponse = new ApiResponseDto(message: "Get User Successful", data: user);
        return Ok(apiResponse);
    }

    [HttpPost("update")]
    [Authorize]
    public async Task<IActionResult> UpdateUser([FromBody] UserInfoDto userInfoDto)
    {
        string idToken = HttpContext.Request.Cookies["token"];
        ApiResponseDto apiResponse = new ApiResponseDto(message: await _userManager.UpdateUser(idToken, userInfoDto));
        return Ok(apiResponse);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePassword)
    {
        string idToken = HttpContext.Request.Cookies["idToken"];
        ApiResponseDto apiResponse =
            new ApiResponseDto(message: await _userManager.ChangePassword(idToken, changePassword));
        return Ok(apiResponse);
    }
}